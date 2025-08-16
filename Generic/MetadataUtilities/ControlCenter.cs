using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using MetadataUtilities.Models;
using MetadataUtilities.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MetadataUtilities
{
    public class ControlCenter
    {
        private readonly MetadataUtilities _plugin;
        private readonly Settings _settings;

        public ControlCenter(MetadataUtilities plugin)
        {
            _plugin = plugin;
            Instance = this;
        }

        public ControlCenter(Settings settings)
        {
            _settings = settings;
            Instance = this;
        }

        public static ControlCenter Instance { get; set; }

        public CopyDataSet GameToCopy { get; set; }

        public bool IsUpdating { get; set; }

        public HashSet<Guid> KnownGames { get; private set; }

        public HashSet<Guid> NewGames { get; } = new HashSet<Guid>();

        public Settings Settings => _plugin?.Settings?.Settings ?? _settings;

        public static MetadataObject AddNewItem(FieldType type, string prefix = "", bool enableTypeSelection = true, bool addToDb = false)
        {
            var newItem = new MetadataObject(type)
            {
                Prefix = prefix
            };

            var window = AddNewObjectViewModel.GetWindow(newItem, enableTypeSelection);

            if (window == null)
            {
                return null;
            }

            if (!(window.ShowDialog() ?? false))
            {
                return null;
            }

            if (addToDb)
            {
                newItem.AddToDb();
            }

            return newItem;
        }

        public static List<MetadataObject> GetItemsFromAddDialog(FieldType type, string prefix = default, bool ignoreEmptyPrefix = true, List<MetadataObject> selectedItems = null)
        {
            var label = type.GetTypeManager().LabelPlural;

            var items = new MetadataObjects(type);

            items.LoadMetadata(false);

            if (prefix != default)
            {
                items.RemoveAll(x => x.Prefix != prefix);
            }
            else if (!ignoreEmptyPrefix)
            {
                items.RemoveAll(x => !string.IsNullOrEmpty(x.Prefix));
            }

            if (selectedItems?.Count > 0)
            {
                foreach (var itemToSelect in selectedItems
                             .Select(item => items.FirstOrDefault(x => x.Name == item.Name && x.Type == item.Type))
                             .Where(itemToSelect => itemToSelect != null))
                {
                    itemToSelect.Selected = true;
                }
            }

            var window = SelectMetadataViewModel.GetWindow(items, label);

            return (window?.ShowDialog() ?? false)
                ? items.Where(x => x.Selected).ToList()
                : new List<MetadataObject>();
        }

        public static void UpdateGames<T>(List<T> games)
        {
            if (games == null || games.Count == 0)
            {
                return;
            }

            var gamesToUpdate = new List<Game>();

            switch (games)
            {
                case List<Game> listOfGames:
                    gamesToUpdate = listOfGames;
                    break;
                case List<Guid> gameIds:
                    {
                        foreach (var gameId in gameIds)
                        {
                            gamesToUpdate.AddMissing(API.Instance.Database.Games[gameId]);
                        }

                        break;
                    }
            }

            if (Instance.Settings?.WriteDebugLog ?? false)
            {
                Log.Debug($"Updating {gamesToUpdate.Count} games:\n{string.Join("\n", gamesToUpdate.Select(g => g.Name))}");
            }

            if (gamesToUpdate.Count == 0)
            {
                return;
            }

            API.Instance.MainView.UIDispatcher.Invoke(delegate
            {
                API.Instance.Database.Games.Update(gamesToUpdate);
            });
        }

        public bool AddNewGame(Guid id) => NewGames.Add(id);

        public void GetKnownGames()
        {
            if (KnownGames != null)
            {
                return;
            }

            KnownGames = API.Instance.Database.Games.Select(x => x.Id).Distinct().ToHashSet();
        }

        public void MergeMetadataObjects(MetadataObject mergeTarget, MetadataObjects items, bool saveAsRule = false)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            var rule = new MergeRule(mergeTarget.Type, mergeTarget.Name)
            {
                Id = mergeTarget.Id,
                SourceObjects = items
            };

            if (saveAsRule)
            {
                Settings.MergeRules.AddRule(rule);
                SavePluginSettings();
            }

            rule.MergeItems();
        }

        public List<MetadataObject> RemoveUnusedMetadata(bool autoMode = false)
        {
            var temporaryList = new List<MetadataObject>();

            var types = Settings.TypeConfigs.Where(x => !autoMode || x.RemoveUnusedItems).ToList();

            if (types.Count == 0)
            {
                return temporaryList;
            }

            var globalProgressOptions = new GlobalProgressOptions(
                ResourceProvider.GetString("LOCMetadataUtilitiesProgressRemovingUnused"),
                false
            )
            {
                IsIndeterminate = true
            };

            API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
            {
                try
                {
                    foreach (var type in types)
                    {
                        if (type.Type.GetTypeManager() is IObjectType objectType)
                        {
                            temporaryList.AddRange(objectType.LoadUnusedMetadata(type.HiddenAsUnused)
                                .Select(x
                                    => new MetadataObject(type.Type, x.Name)
                                    {
                                        Id = x.Id
                                    }));
                        }
                    }

                    if (temporaryList.Count > 0 && (Settings.UnusedItemsWhiteList?.Count ?? 0) > 0)
                    {
                        temporaryList = temporaryList.Where(x =>
                            Settings.UnusedItemsWhiteList.All(y => y.TypeAndName != x.TypeAndName)).ToList();
                    }

                    foreach (var item in temporaryList)
                    {
                        item.RemoveFromDb(types.FirstOrDefault(x => x.Type == item.Type)?.HiddenAsUnused ?? false);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            if (temporaryList.Count != 0)
            {
                if (autoMode)
                {
                    var items = string.Join(Environment.NewLine, temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name).Select(x => x.TypeAndName));

                    API.Instance.Notifications.Add("MetadataUtilities",
                        $"{ResourceProvider.GetString("LOCMetadataUtilitiesNotificationRemovedItems")}{Environment.NewLine}{Environment.NewLine}{items}",
                        NotificationType.Info);
                }
                else
                {
                    var items = string.Join(", ", temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name).Select(x => x.TypeAndName));

                    API.Instance.Dialogs.ShowMessage($"{ResourceProvider.GetString("LOCMetadataUtilitiesNotificationRemovedItems")}{Environment.NewLine}{Environment.NewLine}{items}",
                        ResourceProvider.GetString("LOCMetadataUtilitiesName"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (!autoMode)
            {
                API.Instance.Dialogs.ShowMessage(
                    ResourceProvider.GetString("LOCMetadataUtilitiesDialogNoUnusedItemsFound"),
                    ResourceProvider.GetString("LOCMetadataUtilitiesName"), MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return temporaryList;
        }

        public bool RenameObject(IMetadataFieldType type, string oldName, string newName)
        {
            var mustSave = false;

            if (oldName == newName || oldName == string.Empty)
            {
                return true;
            }

            if (Settings.RenameMergeRules && FieldTypeHelper.ItemListFieldValues().ContainsKey(type.Type))
            {
                mustSave = Settings.MergeRules.FindAndRenameRule(type.Type, oldName,
                    newName);
            }

            if (Settings.RenameConditionalActions)
            {
                foreach (var condAction in Settings.ConditionalActions)
                {
                    foreach (var item in condAction.Conditions)
                    {
                        if (item.Type != type.Type || item.Name != oldName)
                        {
                            continue;
                        }

                        item.Name = newName;
                        mustSave = true;
                    }

                    foreach (var item in condAction.Actions)
                    {
                        if (item.Type != type.Type || item.Name != oldName)
                        {
                            continue;
                        }

                        item.Name = newName;
                        mustSave = true;
                    }
                }
            }

            if (Settings.RenameQuickAdd && type.ValueType == ItemValueType.ItemList)
            {
                foreach (var item in Settings.QuickAddObjects)
                {
                    if (item.Type != type.Type || item.Name != oldName)
                    {
                        continue;
                    }

                    item.Name = newName;
                    mustSave = true;
                }
            }

            if (Settings.RenameWhiteList && type.ValueType == ItemValueType.ItemList)
            {
                foreach (var item in Settings.UnusedItemsWhiteList)
                {
                    if (item.Type != type.Type || item.Name != oldName)
                    {
                        continue;
                    }

                    item.Name = newName;
                    mustSave = true;
                }
            }

            if (mustSave)
            {
                Instance.SavePluginSettings();
            }

            return true;
        }

        public void ResetNewGames()
        {
            KnownGames.UnionWith(NewGames);
            NewGames.Clear();
        }

        public void OpenCopyMetadataWindow(Game sourceGame)
        {
            var window = CopyMetadataViewModel.GetWindow(sourceGame);

            window?.ShowDialog();
        }

        public void SavePluginSettings() => _plugin?.SavePluginSettings(Settings);
    }
}
