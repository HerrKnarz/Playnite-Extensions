using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using MetadataUtilities.Actions;
using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using MetadataUtilities.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MergeAction = MetadataUtilities.Actions.MergeAction;

namespace MetadataUtilities
{
    public static class MetadataFunctions
    {
        public static void DoForAll(MetadataUtilities plugin, List<MyGame> games, IBaseAction action,
            bool showDialog = false, ActionModifierType actionModifier = ActionModifierType.None, object item = null)
        {
            plugin.IsUpdating = true;
            var gamesAffected = 0;

            if (plugin.Settings.Settings.WriteDebugLog)
            {
                Log.Debug($"===> Started {action.GetType()} for {games.Count} games. =======================");
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (games.Count == 1)
                {
                    if (action.Execute(games.First(), actionModifier, item, false))
                    {
                        gamesAffected++;
                    }

                    action.FollowUp(actionModifier, item, false);
                }
                // if we have more than one game in the list, we want to start buffered mode and
                // show a progress bar.
                else if (games.Count > 1)
                {
                    using (plugin.PlayniteApi.Database.BufferedUpdate())
                    {
                        if (!action.Prepare(actionModifier, item))
                        {
                            return;
                        }

                        var globalProgressOptions = new GlobalProgressOptions(
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

                                foreach (var game in games)
                                {
                                    activateGlobalProgress.Text =
                                        $"{ResourceProvider.GetString("LOCMetadataUtilitiesName")}{Environment.NewLine}{action.ProgressMessage}{Environment.NewLine}{game.Game.Name}";

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
                if (plugin.Settings.Settings.WriteDebugLog)
                {
                    Log.Debug($"===> Finished {action.GetType()} with {gamesAffected} games affected. =======================");
                }

                plugin.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }

        public static List<MetadataObject> GetItemsFromAddDialog(FieldType type, Settings settings)
        {
            var label = type.GetTypeManager().LabelPlural;

            var items = new MetadataObjects(settings, type);

            items.LoadMetadata(false);

            var window = SelectMetadataViewModel.GetWindow(items, label);

            return (window?.ShowDialog() ?? false)
                ? items.Where(x => x.Selected).ToList()
                : new List<MetadataObject>();
        }

        public static void MergeItems(MetadataUtilities plugin, MergeRule rule, bool showDialog = true)
        {
            plugin.IsUpdating = true;
            Cursor.Current = Cursors.WaitCursor;
            var gamesAffected = new List<Guid>();
            try
            {
                using (API.Instance.Database.BufferedUpdate())
                {
                    var globalProgressOptions = new GlobalProgressOptions(
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
                            gamesAffected.AddMissing(rule.Merge(API.Instance.Database.Games.ToList()));
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
            if (!showDialog)
            {
                return;
            }

            API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogMergedMetadataMessage"), gamesAffected.Distinct().Count()));
        }

        public static List<MetadataObject> RemoveUnusedMetadata(Settings settings, bool autoMode = false)
        {
            var temporaryList = new List<MetadataObject>();

            if (API.Instance == null || API.Instance.Dialogs == null)

            {
                return temporaryList;
            }

            var types = FieldTypeHelper.GetItemListTypes().ToList();

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
                        temporaryList.AddRange(type.LoadUnusedMetadata(settings.IgnoreHiddenGamesInRemoveUnused).Select(x
                            => new MetadataObject(settings, type.Type, x.Name)
                            {
                                Id = x.Id
                            }));
                    }

                    if (temporaryList.Count > 0 && (settings.UnusedItemsWhiteList?.Count ?? 0) > 0)
                    {
                        temporaryList = temporaryList.Where(x =>
                            settings.UnusedItemsWhiteList.All(y => y.TypeAndName != x.TypeAndName)).ToList();
                    }

                    foreach (var item in temporaryList)
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

        public static void RenameObject(MetadataUtilities plugin, IMetadataFieldType type, string oldName, string newName)
        {
            var mustSave = false;

            if (oldName == newName || oldName == string.Empty)
            {
                return;
            }

            if (plugin.Settings.Settings.RenameMergeRules && FieldTypeHelper.ItemListFieldValues().ContainsKey(type.Type))
            {
                mustSave = plugin.Settings.Settings.MergeRules.FindAndRenameRule(type.Type, oldName,
                    newName);
            }

            if (plugin.Settings.Settings.RenameConditionalActions)
            {
                foreach (var condAction in plugin.Settings.Settings.ConditionalActions)
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

            if (plugin.Settings.Settings.RenameQuickAdd && type.ValueType == ItemValueType.ItemList)
            {
                foreach (var item in plugin.Settings.Settings.QuickAddObjects)
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
                foreach (var item in plugin.Settings.Settings.UnusedItemsWhiteList)
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

        public static void UpdateGames<T>(List<T> games, Settings settings)
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

            if (MergeAction.Instance(settings).Settings?.WriteDebugLog ?? false)
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
    }
}