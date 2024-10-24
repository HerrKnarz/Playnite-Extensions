using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MetadataUtilities.Models
{
    public class MetadataObjects : ObservableCollection<MetadataObject>
    {
        private readonly IObjectType _typeManager;

        /// <summary>
        /// needed for deserializing the settings.
        /// </summary>
        public MetadataObjects()
        { }

        public MetadataObjects(FieldType? type = null)
        {
            if (type?.GetTypeManager() is IObjectType objectType)
            {
                _typeManager = objectType;
            }
        }

        [DontSerialize]
        public Settings Settings => ControlCenter.Instance.Settings;

        public void AddItems(FieldType type)
        {
            var items = MetadataFunctions.GetItemsFromAddDialog(type);

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

            foreach (var item in items.Where(item => this.All(x => x.TypeAndName != item.TypeAndName)))
            {
                Add(new MetadataObject(item.Type, item.Name));
            }

            this.Sort(x => x.TypeAndName);
        }

        public MetadataObject AddNewItem(FieldType type, string prefix = "", bool enableTypeSelection = true, bool addToDb = false)
        {
            var newItem = MetadataFunctions.AddNewItem(type, prefix, enableTypeSelection, addToDb);

            if (this.Any(x => x.TypeAndName == newItem.TypeAndName))
            {
                return null;
            }

            Add(newItem);

            this.Sort(x => x.TypeAndName);

            return newItem;
        }

        public MetadataObject GetSibling(MetadataObject item)
        {
            if (!(item.TypeManager is BaseCompanyType))
            {
                return null;
            }

            var otherType = item.Type == FieldType.Developer
                ? FieldType.Publisher
                : FieldType.Developer;

            return this.FirstOrDefault(x => x.Name == item.Name && x.Type == otherType);
        }

        public void LoadGameMetadata(List<Game> games)
        {
            var temporaryList = new List<MetadataObject>();

            var globalProgressOptions = new GlobalProgressOptions(
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
                    var types = Settings.TypeConfigs.Where(x => x.Selected).ToList();

                    foreach (var game in games)
                    {
                        foreach (var type in types)
                        {
                            if (type.Type.GetTypeManager() is IObjectType objectType)
                            {
                                temporaryList.AddMissing(objectType.LoadGameMetadata(game).Select(x =>
                                    new MetadataObject(type.Type, x.Name)
                                    {
                                        Id = x.Id
                                    }));
                            }
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
            if (Settings.WriteDebugLog)
            {
                Log.Debug("=== LoadMetadata: Start ===");
            }

            var ts = DateTime.Now;

            var temporaryList = new List<MetadataObject>();

            var types = new List<IObjectType>();

            if (_typeManager != null)
            {
                types.Add(_typeManager);
            }
            else if (onlyMergeAble)
            {
                types.AddRange(Settings.TypeConfigs.Where(x => x.Selected)
                    .Select(x => x.Type.GetTypeManager() as IObjectType).ToList());
            }
            else
            {
                types.AddRange(FieldTypeHelper.GetAllTypes<IObjectType>());
            }

            var globalProgressOptions = new GlobalProgressOptions(
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
                    foreach (var typeManager in types)
                    {
                        temporaryList.AddRange(typeManager.LoadAllMetadata().Select(x => new MetadataObject(typeManager.Type, x.Name)
                        {
                            Id = x.Id
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

            if (Settings.WriteDebugLog)
            {
                Log.Debug($"=== LoadMetadata: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
            }
        }

        public bool Remove(MetadataObject item, bool removeSibling)
        {
            var result = Remove(item);

            if (!removeSibling)
            {
                return result;
            }

            if (!result || !(item.TypeManager is BaseCompanyType))
            {
                return result;
            }

            var sibling = GetSibling(item);

            if (sibling != null)
            {
                Remove(sibling);
            }

            return true;
        }

        public void RemoveItems(IEnumerable<MetadataObject> items)
        {
            foreach (var item in items.ToList().Cast<MetadataObject>())
            {
                Remove(item);
            }
        }

        private void UpdateGameCounts(IEnumerable<MetadataObject> itemList, bool ignoreHiddenGames, IMetadataFieldType typeManager = null, bool onlyMergeAble = true)
        {
            if (Settings.WriteDebugLog)
            {
                Log.Debug("=== UpdateGameCounts: Start ===");
            }

            var ts = DateTime.Now;

            var maxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0));

            var types = new ConcurrentDictionary<FieldType, ConcurrentQueue<Guid>>(maxDegreeOfParallelism, 10);

            if (typeManager != null)
            {
                types.TryAdd(typeManager.Type, new ConcurrentQueue<Guid>());
            }
            else if (onlyMergeAble)
            {
                foreach (var type in FieldTypeHelper.GetItemListTypes())
                {
                    types.TryAdd(type.Type, new ConcurrentQueue<Guid>());
                }
            }
            else
            {
                foreach (var type in FieldTypeHelper.GetAllTypes<IObjectType>())
                {
                    types.TryAdd(type.Type, new ConcurrentQueue<Guid>());
                }
            }

            var opts = new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0)) };

            if (typeManager != null)
            {
                Parallel.ForEach(itemList, opts, item => item.GetGameCount());

                return;
            }

            Parallel.ForEach(API.Instance.Database.Games.Where(g => !(ignoreHiddenGames && g.Hidden)), opts, game =>
            {
                foreach (var type in types)
                {
                    if (type.Key.GetTypeManager() is IObjectType objectType)
                    {
                        objectType.LoadGameMetadata(game).ForEach(o => type.Value.Enqueue(o.Id));
                    }
                }
            });

            var li = types.ToDictionary(type => type.Key, type => type.Value.GroupBy(i => i).ToList());

            Parallel.ForEach(itemList, opts, item =>
            {
                if (li.TryGetValue(item.Type, out var value))
                {
                    item.GameCount = value.FirstOrDefault(i => i.Key == item.Id)?.Count() ?? 0;
                }
            });

            if (Settings.WriteDebugLog)
            {
                Log.Debug($"=== UpdateGameCounts: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
            }
        }
    }
}