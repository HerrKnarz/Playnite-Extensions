using KNARZhelper;
using KNARZhelper.Enum;
using MetadataUtilities.Models;
using MetadataUtilities.ViewModels;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using KNARZhelper.DatabaseObjectTypes;
using Action = MetadataUtilities.Models.Action;
using MetadataUtilities.Actions;
using MetadataUtilities.Enums;
using Playnite.SDK.Models;

namespace MetadataUtilities
{
    public static class MetadataFunctions
    {
        public static void DoForAll(MetadataUtilities plugin, List<Game> games, IBaseAction action, bool showDialog = false, ActionModifierType actionModifier = ActionModifierType.None, object item = null)
        {
            plugin.IsUpdating = true;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (games.Count == 1)
                {
                    action.Execute(games.First(), actionModifier, item, false);
                    action.FollowUp(actionModifier, item, false);
                }
                // if we have more than one game in the list, we want to start buffered mode and
                // show a progress bar.
                else if (games.Count > 1)
                {
                    int gamesAffected = 0;

                    using (plugin.PlayniteApi.Database.BufferedUpdate())
                    {
                        if (!action.Prepare(actionModifier, item))
                        {
                            return;
                        }

                        GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                            $"{ResourceProvider.GetString("LOCMetadataUtilitiesName")} - {action.ProgressMessage}",
                            true
                        )
                        {
                            IsIndeterminate = false
                        };

                        plugin.PlayniteApi.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                        {
                            try
                            {
                                activateGlobalProgress.ProgressMaxValue = games.Count;

                                foreach (Game game in games)
                                {
                                    activateGlobalProgress.Text =
                                        $"{ResourceProvider.GetString("LOCMetadataUtilitiesName")}{Environment.NewLine}{action.ProgressMessage}{Environment.NewLine}{game.Name}";

                                    if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    if (action.Execute(game, actionModifier, item))
                                    {
                                        gamesAffected++;
                                    }

                                    activateGlobalProgress.CurrentProgressValue++;
                                }

                                action.FollowUp(actionModifier, item);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex);
                            }
                        }, globalProgressOptions);
                    }

                    // Shows a dialog with the number of games actually affected.
                    if (!showDialog)
                    {
                        return;
                    }

                    Cursor.Current = Cursors.Default;
                    plugin.PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString(action.ResultMessage), gamesAffected));
                }
            }
            finally
            {
                plugin.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }

        public static List<MetadataObject> GetItemsFromAddDialog(FieldType type, Settings settings)
        {
            string label = type.GetTypeManager().LabelPlural;

            MetadataObjects items = new MetadataObjects(settings, type);

            items.LoadMetadata(false);

            Window window = SelectMetadataViewModel.GetWindow(items, label);

            return (window?.ShowDialog() ?? false)
                ? items.Where(x => x.Selected).ToList()
                : new List<MetadataObject>();
        }

        public static void MergeItems(MetadataUtilities plugin, List<Game> games, MergeRule rule) =>
            MergeItems(plugin, games, new List<MergeRule> { rule }, true);

        public static void MergeItems(MetadataUtilities plugin, List<Game> games = null, List<MergeRule> rules = null, bool showDialog = false)
        {
            List<Guid> gamesAffected = new List<Guid>();

            plugin.IsUpdating = true;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using (API.Instance.Database.BufferedUpdate())
                {
                    GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                        ResourceProvider.GetString("LOCMetadataUtilitiesProgressMergingItems"),
                        false
                    )
                    {
                        IsIndeterminate = true
                    };

                    API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                    {
                        try
                        {
                            if (games == null)
                            {
                                games = new List<Game>();
                                games.AddRange(API.Instance.Database.Games);
                            }

                            if (rules == null)
                            {
                                rules = new List<MergeRule>();
                                rules.AddRange(plugin.Settings.Settings.MergeRules);
                            }

                            foreach (MergeRule rule in rules)
                            {
                                gamesAffected.AddMissing(rule.Merge(games));
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }, globalProgressOptions);
                }
            }
            finally
            {
                plugin.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }

            // Shows a dialog with the number of games actually affected.
            if (!showDialog || games?.Count == 1)
            {
                return;
            }

            API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogMergedMetadataMessage"), gamesAffected.Distinct().Count()));
        }

        public static List<MetadataObject> RemoveUnusedMetadata(Settings settings, bool autoMode = false)
        {
            List<MetadataObject> temporaryList = new List<MetadataObject>();

            List<IEditableObjectType> types = FieldTypeHelper.GetItemListTypes().ToList();

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
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
                    foreach (IEditableObjectType type in types)
                    {
                        temporaryList.AddRange(type.LoadUnusedMetadata(settings.IgnoreHiddenGamesInRemoveUnused).Select(x
                            => new MetadataObject(settings)
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Type = type.Type
                            }));
                    }

                    if (temporaryList.Count > 0 && (settings.UnusedItemsWhiteList?.Count ?? 0) > 0)
                    {
                        temporaryList = temporaryList.Where(x =>
                            settings.UnusedItemsWhiteList.All(y => y.TypeAndName != x.TypeAndName)).ToList();
                    }

                    foreach (MetadataObject item in temporaryList)
                    {
                        item.RemoveFromDb(settings.IgnoreHiddenGamesInRemoveUnused);
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
                    string items = string.Join(Environment.NewLine, temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name).Select(x => x.TypeAndName));

                    API.Instance.Notifications.Add("MetadataUtilities",
                        $"{ResourceProvider.GetString("LOCMetadataUtilitiesNotificationRemovedItems")}{Environment.NewLine}{Environment.NewLine}{items}",
                        NotificationType.Info);
                }
                else
                {
                    string items = string.Join(", ", temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name).Select(x => x.TypeAndName));
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

        public static void RenameObject(MetadataUtilities plugin, IMetadataFieldType type, string oldName, string newName)
        {
            bool mustSave = false;

            if (oldName == newName || oldName == string.Empty)
            {
                return;
            }

            if (plugin.Settings.Settings.RenameDefaults && (type.Type == FieldType.Category || type.Type == FieldType.Tag))
            {
                MetadataObject tag = plugin.Settings.Settings.DefaultTags?.FirstOrDefault(x => x.Name == oldName);

                if (tag != null)
                {
                    tag.Name = newName;
                    mustSave = true;
                }

                MetadataObject category = plugin.Settings.Settings.DefaultCategories?.FirstOrDefault(x => x.Name == oldName);

                if (category != null)
                {
                    category.Name = newName;
                    mustSave = true;
                }
            }

            if (plugin.Settings.Settings.RenameMergeRules && FieldTypeHelper.ItemListFieldValues().ContainsKey(type.Type))
            {
                mustSave = plugin.Settings.Settings.MergeRules.FindAndRenameRule(type.Type, oldName,
                    newName);
            }

            if (plugin.Settings.Settings.RenameConditionalActions)
            {
                foreach (ConditionalAction condAction in plugin.Settings.Settings.ConditionalActions)
                {
                    foreach (Models.Condition item in condAction.Conditions)
                    {
                        if (item.Type != type.Type || item.Name != oldName)
                        {
                            continue;
                        }

                        item.Name = newName;
                        mustSave = true;
                    }

                    foreach (Action item in condAction.Actions)
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

            if (plugin.Settings.Settings.RenameQuickAdd && type.ValueType == ItemValueType.ItemList)
            {
                foreach (QuickAddObject item in plugin.Settings.Settings.QuickAddObjects)
                {
                    if (item.Type != type.Type || item.Name != oldName)
                    {
                        continue;
                    }

                    item.Name = newName;
                    mustSave = true;
                }
            }

            if (plugin.Settings.Settings.RenameWhiteList && type.ValueType == ItemValueType.ItemList)
            {
                foreach (MetadataObject item in plugin.Settings.Settings.UnusedItemsWhiteList)
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
                plugin.SavePluginSettings(plugin.Settings.Settings);
            }
        }
    }
}