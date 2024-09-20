using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using MetadataUtilities.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MetadataUtilities.Models
{
    public class MetadataObjects : ObservableCollection<MetadataObject>
    {
        private readonly IObjectType _typeManager;

        /// <summary>
        /// Only used for deserializing the settings. Needs to use the "ResetReferences" method of
        /// the settings afterward!
        /// </summary>
        public MetadataObjects()
        { }

        public MetadataObjects(Settings settings, FieldType? type = null)
        {
            Settings = settings;

            if (type?.GetTypeManager() is IObjectType objectType)
            {
                _typeManager = objectType;
            }
        }

        [DontSerialize]
        public Settings Settings { get; set; }

        public void AddItems(FieldType type)
        {
            List<MetadataObject> items = MetadataFunctions.GetItemsFromAddDialog(type, Settings);

            if (items.Count == 0)
            {
                return;
            }

            AddItems(items);
        }

        public void AddItems(List<MetadataObject> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            foreach (MetadataObject item in items.Where(item => this.All(x => x.TypeAndName != item.TypeAndName)))
            {
                Add(new MetadataObject(Settings)
                {
                    Name = item.Name,
                    Type = item.Type
                });
            }

            this.Sort(x => x.TypeAndName);
        }

        public MetadataObject AddNewItem(FieldType type, string prefix = "", bool enableTypeSelection = true, bool addToDb = false)
        {
            MetadataObject newItem = new MetadataObject(Settings)
            {
                Type = type,
                Prefix = prefix
            };

            Window window = AddNewObjectViewModel.GetWindow(Settings, newItem, enableTypeSelection);

            if (window == null)
            {
                return null;
            }

            if (!(window.ShowDialog() ?? false))
            {
                return null;
            }

            if (this.Any(x => x.TypeAndName == newItem.TypeAndName))
            {
                return null;
            }

            if (addToDb)
            {
                newItem.AddToDb();
            }

            Add(newItem);

            this.Sort(x => x.TypeAndName);

            return newItem;
        }

        public void LoadGameMetadata(List<Game> games)
        {
            List<MetadataObject> temporaryList = new List<MetadataObject>();

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                ResourceProvider.GetString("LOCLoadingLabel"),
                false
            )
            {
                IsIndeterminate = true
            };

            API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
            {
                try
                {
                    List<IEditableObjectType> types = FieldTypeHelper.GetItemListTypes().ToList();

                    foreach (Game game in games)
                    {
                        foreach (IEditableObjectType type in types)
                        {
                            temporaryList.AddMissing(type.LoadGameMetadata(game).Select(x =>
                                new MetadataObject(Settings)
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    Type = type.Type
                                }));
                        }
                    }

                    UpdateGameCounts(temporaryList, Settings.IgnoreHiddenGamesInGameCount);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            Clear();
            this.AddMissing(temporaryList.Distinct(new MetadataObjectEqualityComparer()).OrderBy(x => x.TypeLabel)
                .ThenBy(x => x.Name));
        }

        public void LoadMetadata(bool showGameNumber = true, bool onlyMergeAble = true)
        {
            Log.Debug("=== LoadMetadata: Start ===");
            DateTime ts = DateTime.Now;

            List<MetadataObject> temporaryList = new List<MetadataObject>();

            List<IObjectType> types = new List<IObjectType>();

            if (_typeManager != null)
            {
                types.Add(_typeManager);
            }
            else if (onlyMergeAble)
            {
                types.AddRange(FieldTypeHelper.GetItemListTypes());
            }
            else
            {
                types.AddRange(FieldTypeHelper.GetAllTypes<IObjectType>());
            }

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                ResourceProvider.GetString("LOCLoadingLabel"),
                false
            )
            {
                IsIndeterminate = true
            };

            API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
            {
                try
                {
                    foreach (IObjectType typeManager in types)
                    {
                        temporaryList.AddRange(typeManager.LoadAllMetadata().Select(x => new MetadataObject(Settings)
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Type = typeManager.Type
                        }));
                    }

                    if (showGameNumber)
                    {
                        UpdateGameCounts(temporaryList, Settings.IgnoreHiddenGamesInGameCount, _typeManager, onlyMergeAble);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            Clear();
            this.AddMissing(temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name));
            Log.Debug($"=== LoadMetadata: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
        }

        public void RemoveItems(IEnumerable<MetadataObject> items)
        {
            foreach (MetadataObject item in items.ToList().Cast<MetadataObject>())
            {
                Remove(item);
            }
        }

        private static void UpdateGameCounts(IEnumerable<MetadataObject> itemList, bool ignoreHiddenGames, IObjectType typeManager = null, bool onlyMergeAble = true)
        {
            Log.Debug("=== UpdateGameCounts: Start ===");
            DateTime ts = DateTime.Now;

            List<IObjectType> types = new List<IObjectType>();

            if (typeManager != null)
            {
                types.Add(typeManager);
            }
            else if (onlyMergeAble)
            {
                types.AddRange(FieldTypeHelper.GetItemListTypes());
            }
            else
            {
                types.AddRange(FieldTypeHelper.GetAllTypes<IObjectType>());
            }

            ParallelOptions opts = new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0)) };

            if (typeManager != null)
            {
                Parallel.ForEach(itemList, opts, item => item.GetGameCount());

                return;
            }

            ConcurrentQueue<Guid> items = new ConcurrentQueue<Guid>();

            Parallel.ForEach(API.Instance.Database.Games.Where(g => !(ignoreHiddenGames && g.Hidden)), opts, game =>
            {
                foreach (IObjectType type in types)
                {
                    type.LoadGameMetadata(game).ForEach(o => items.Enqueue(o.Id));
                }
            });

            List<IGrouping<Guid, Guid>> li = items.GroupBy(i => i).ToList();

            Parallel.ForEach(itemList, opts, item => item.GameCount = li.FirstOrDefault(i => i.Key == item.Id)?.Count() ?? 0);

            Log.Debug($"=== UpdateGameCounts: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
        }
    }
}