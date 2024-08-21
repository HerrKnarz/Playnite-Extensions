using KNARZhelper;
using KNARZhelper.Enum;
using MetadataUtilities.Actions;
using MetadataUtilities.Models;
using MetadataUtilities.ViewModels;
using MetadataUtilities.Views;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;

namespace MetadataUtilities
{
    public class MetadataUtilities : GenericPlugin
    {
        public MetadataUtilities(IPlayniteAPI api) : base(api)
        {
            Settings = new SettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            IsUpdating = false;

            api.Database.Games.ItemUpdated += Games_ItemUpdated;
            api.Database.AgeRatings.ItemUpdated += ItemUpdatedAgeRatings;
            api.Database.Categories.ItemUpdated += ItemUpdatedCategories;
            api.Database.Features.ItemUpdated += ItemUpdatedFeatures;
            api.Database.Genres.ItemUpdated += ItemUpdatedGenres;
            api.Database.Series.ItemUpdated += ItemUpdatedSeries;
            api.Database.Tags.ItemUpdated += ItemUpdatedTags;

            Dictionary<string, string> iconResourcesToAdd = new Dictionary<string, string>
            {
                { "muEditorIcon", "\xf005" },
                { "muMergeIcon", "\xef29" },
                { "muRemoveIcon", "\xee09" },
                { "muTagIcon", "\xf004" },
                { "muAllCheckedIcon", "\xeed8" },
                { "muSomeCheckedIcon", "\xeed7" }
            };

            foreach (KeyValuePair<string, string> iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }
        }

        public override Guid Id { get; } = Guid.Parse("485ab5f0-bfb1-4c17-93cc-20d8338673be");
        public SettingsViewModel Settings { get; }
        internal bool IsUpdating { get; set; }

        public void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            if (!Settings.Settings.MergeMetadataOnMetadataUpdate || IsUpdating)
            {
                return;
            }

            // Only run for games, that have values in one of the supported fields and those differ
            // from the ones before.
            List<Game> games = args.UpdatedItems.Where(item =>
                item.OldData == null ||
                (item.NewData.CategoryIds != null &&
                 (item.OldData.CategoryIds == null ||
                  !new HashSet<Guid>(item.OldData.CategoryIds).SetEquals(item.NewData.CategoryIds))) ||
                (item.NewData.FeatureIds != null &&
                 (item.OldData.FeatureIds == null ||
                  !new HashSet<Guid>(item.OldData.FeatureIds).SetEquals(item.NewData.FeatureIds))) ||
                (item.NewData.GenreIds != null &&
                 (item.OldData.GenreIds == null ||
                  !new HashSet<Guid>(item.OldData.GenreIds).SetEquals(item.NewData.GenreIds))) ||
                (item.NewData.SeriesIds != null &&
                 (item.OldData.SeriesIds == null ||
                  !new HashSet<Guid>(item.OldData.SeriesIds).SetEquals(item.NewData.SeriesIds))) ||
                (item.NewData.TagIds != null &&
                 (item.OldData.TagIds == null ||
                  !new HashSet<Guid>(item.OldData.TagIds).SetEquals(item.NewData.TagIds)))).Select(item => item.NewData).ToList();

            if (Settings.Settings.RemoveUnwantedOnMetadataUpdate)
            {
                DoForAll(games, RemoveUnwantedAction.Instance(this));
            }

            MergeItems(games);

            DoForAll(games, ExecuteConditionalActionsAction.Instance(this));
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            string menuSection = ResourceProvider.GetString("LOCMetadataUtilitiesName");
            string mergeSection = ResourceProvider.GetString("LOCMetadataUtilitiesSettingsMergeRules");
            string conditionalSection = ResourceProvider.GetString("LOCMetadataUtilitiesSettingsTabConditionalActions");
            List<GameMenuItem> menuItems = new List<GameMenuItem>();
            List<Game> games = args.Games.Distinct().ToList();

            GameMenuItem item = new GameMenuItem
            {
                Description = "",
                MenuSection = ResourceProvider.GetString("LOCUserScore"),
                Action = a =>
                    DoForAll(games, SetUserScoreAction.Instance(this), true, ActionModifierTypes.None, 0)
            };
            menuItems.Add(item);

            for (int i = 1; i <= 10; i++)
            {
                int rating = i * 10;
                GameMenuItem menuItem = new GameMenuItem
                {
                    Description = new string('\u2605', i),
                    MenuSection = ResourceProvider.GetString("LOCUserScore"),
                    Action = a =>
                        DoForAll(games, SetUserScoreAction.Instance(this), true, ActionModifierTypes.None, rating)
                };
                menuItems.Add(menuItem);
            }

            menuItems.AddRange(new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuEditor"),
                    MenuSection = menuSection,
                    Icon = "muEditorIcon",
                    Action = a => ShowEditor(games)
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuAddDefaults"),
                    MenuSection = menuSection,
                    Icon = "muTagIcon",
                    Action = a => DoForAll(games, AddDefaultsAction.Instance(this), true)
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuRemoveUnwanted"),
                    MenuSection = menuSection,
                    Icon = "muRemoveIcon",
                    Action = a => DoForAll(games, RemoveUnwantedAction.Instance(this), true)
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuMergeMetadata"),
                    MenuSection = menuSection,
                    Icon = "muMergeIcon",
                    Action = a => MergeItems(games, null, true)
                }
            });

            menuItems.AddRange(Settings.Settings.MergeRules.OrderBy(x => x.TypeAndName).Select(rule => new GameMenuItem
            {
                Description = rule.TypeAndName,
                MenuSection = $"{menuSection}|{mergeSection}",
                Action = a => MergeItems(games, rule)
            }));

            menuItems.AddRange(Settings.Settings.ConditionalActions.OrderBy(x => x.Name).Select(action => new GameMenuItem
            {
                Description = action.Name,
                MenuSection = $"{menuSection}|{conditionalSection}",
                Action = a => DoForAll(games, ExecuteConditionalActionsAction.Instance(this), true,
                    ActionModifierTypes.IsManual, action)
            }));

            List<GameMenuItem> quickAddItems = new List<GameMenuItem>();

            string baseMenu = Settings.Settings.QuickAddSingleMenuEntry
                ? ResourceProvider.GetString("LOCMetadataUtilitiesName") + "|"
                : "";

            if (Settings.Settings.QuickAddObjects.Count == 0)
            {
                return menuItems;
            }

            quickAddItems.AddRange(CreateMenuItems(baseMenu, games, Settings.Settings.QuickAddObjects));
            quickAddItems.AddRange(CreateMenuItems(baseMenu, games, Settings.Settings.QuickAddObjects, ActionModifierTypes.Remove));
            quickAddItems.AddRange(CreateMenuItems(baseMenu, games, Settings.Settings.QuickAddObjects, ActionModifierTypes.Toggle));

            if (quickAddItems.Count == 0)
            {
                return menuItems;
            }

            if (Settings.Settings.QuickAddSingleMenuEntry)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = "-",
                    MenuSection = $"{menuSection}"
                }
                );
            }

            menuItems.AddRange(quickAddItems.OrderBy(x => x.MenuSection).ThenBy(x => x.Description));

            return menuItems;
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            string menuSection = ResourceProvider.GetString("LOCMetadataUtilitiesName");

            List<MainMenuItem> menuItems = new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuEditor"),
                    MenuSection = $"@{menuSection}",
                    Icon = "muEditorIcon",
                    Action = a => ShowEditor()
                },
                new MainMenuItem
                {
                    Description = "-",
                    MenuSection = $"@{menuSection}"
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuRemoveUnused"),
                    MenuSection = $"@{menuSection}",
                    Icon = "muRemoveIcon",
                    Action = a => RemoveUnused()
                }
            };

            return menuItems;
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new SettingsView();

        public override IEnumerable<TopPanelItem> GetTopPanelItems()
        {
            if (Settings.Settings.ShowTopPanelButton)
            {
                yield return new TopPanelItem
                {
                    Icon = new TextBlock
                    {
                        Text = char.ConvertFromUtf32(0xf005),
                        FontSize = 20,
                        FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
                    },
                    Visible = true,
                    Title = ResourceProvider.GetString("LOCMetadataUtilitiesMenuEditor"),
                    Activated = ShowEditor
                };
            }

            if (!Settings.Settings.ShowTopPanelSettingsButton)
            {
                yield break;
            }

            yield return new TopPanelItem
            {
                Icon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "icon.png"),
                Visible = true,
                Title = $"{ResourceProvider.GetString("LOCMetadataUtilitiesName")}: {ResourceProvider.GetString("LOCMenuPlayniteSettingsTitle")} ",
                Activated = ShowSettings
            };
        }

        public void MergeItems(List<Game> games, MergeRule rule) => MergeItems(games, new List<MergeRule> { rule }, true);

        public void MergeItems(List<Game> games = null, List<MergeRule> rules = null, bool showDialog = false)
        {
            List<Guid> gamesAffected = new List<Guid>();

            IsUpdating = true;
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
                                rules.AddRange(Settings.Settings.MergeRules);
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
                IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }

            // Shows a dialog with the number of games actually affected.
            if (!showDialog || games?.Count == 1)
            {
                return;
            }

            Cursor.Current = Cursors.Default;
            PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogMergedMetadataMessage"), gamesAffected.Distinct().Count()));
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            base.OnApplicationStarted(args);

            if (!Settings.Settings.RemoveUnusedOnStartup)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                MetadataFunctions.RemoveUnusedMetadata(Settings.Settings, true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            List<Game> games = PlayniteApi.Database.Games
                .Where(x => x.Added != null && x.Added > Settings.Settings.LastAutoLibUpdate).ToList();

            DoForAll(games, AddDefaultsAction.Instance(this));

            Settings.Settings.LastAutoLibUpdate = DateTime.Now;

            SavePluginSettings(Settings.Settings);
        }

        public void RemoveUnused()
        {
            Cursor.Current = Cursors.WaitCursor;
            IsUpdating = true;

            try
            {
                MetadataFunctions.RemoveUnusedMetadata(Settings.Settings);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                IsUpdating = false;
            }
        }

        /// <summary>
        /// Executes a specific action for all games in a list. Shows a progress bar and result
        /// dialog and uses buffered update mode if the list contains more than one game.
        /// </summary>
        /// <param name="games">List of games to be processed</param>
        /// <param name="action">Instance of the action to be executed</param>
        /// <param name="showDialog">If true a dialog will be shown after completion</param>
        /// <param name="actionModifier">
        /// specifies the type of action to execute, if more than one is possible.
        /// </param>
        /// <param name="item">item to process. The type is determined by the action type modifier.</param>
        internal void DoForAll(List<Game> games, IBaseAction action, bool showDialog = false, ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null)
        {
            IsUpdating = true;

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

                    using (PlayniteApi.Database.BufferedUpdate())
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

                        PlayniteApi.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
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
                    PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString(action.ResultMessage), gamesAffected));
                }
            }
            finally
            {
                IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private IEnumerable<GameMenuItem> CreateMenuItems(string baseMenu, List<Game> games, IReadOnlyCollection<QuickAddObject> dbObjects, ActionModifierTypes action = ActionModifierTypes.Add)
        {
            List<GameMenuItem> menuItems = new List<GameMenuItem>();

            if (dbObjects.Count == 0)
            {
                return menuItems;
            }

            foreach (QuickAddObject dbObject in dbObjects
                .Where(x => (action == ActionModifierTypes.Add && x.Add) ||
                            (action == ActionModifierTypes.Remove && x.Remove) ||
                            (action == ActionModifierTypes.Toggle && x.Toggle)))
            {
                int checkedCount;

                string customMenu = dbObject.CustomPath?.Trim().Length > 0
                    ? dbObject.CustomPath.Replace("{type}", dbObject.Type.ToString()).Replace("{action}", action.ToString())
                    : Settings.Settings.QuickAddCustomPath?.Trim().Length > 0
                        ? Settings.Settings.QuickAddCustomPath.Replace("{type}", dbObject.Type.ToString()).Replace("{action}", action.ToString())
                        : string.Format(ResourceProvider.GetString($"LOCMetadataUtilitiesMenuQuickAdd{action}"), ResourceProvider.GetString($"LOC{dbObject.Type}Label"));

                switch (dbObject.Type)
                {
                    case FieldType.AgeRating:
                        checkedCount = games.Count(x => x.AgeRatingIds?.Contains(dbObject.Id) ?? false);
                        break;

                    case FieldType.Category:
                        checkedCount = games.Count(x => x.CategoryIds?.Contains(dbObject.Id) ?? false);
                        break;

                    case FieldType.Feature:
                        checkedCount = games.Count(x => x.FeatureIds?.Contains(dbObject.Id) ?? false);
                        break;

                    case FieldType.Genre:
                        checkedCount = games.Count(x => x.GenreIds?.Contains(dbObject.Id) ?? false);
                        break;

                    case FieldType.Series:
                        checkedCount = games.Count(x => x.SeriesIds?.Contains(dbObject.Id) ?? false);
                        break;

                    case FieldType.Tag:
                        checkedCount = games.Count(x => x.TagIds?.Contains(dbObject.Id) ?? false);
                        break;

                    case FieldType.CompletionStatus:
                    case FieldType.Developer:
                    case FieldType.Library:
                    case FieldType.Platform:
                    case FieldType.Publisher:
                    case FieldType.Source:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dbObject.Type), dbObject.Type, null);
                }

                menuItems.Add(new GameMenuItem
                {
                    Icon = checkedCount == games.Count ? "muAllCheckedIcon" :
                        checkedCount > 0 ? "muSomeCheckedIcon" : "",
                    Description = dbObject.Name,
                    MenuSection = baseMenu + customMenu,
                    Action = a => DoForAll(games, QuickAddAction.Instance(this), true, action, dbObject)
                });
            }

            return menuItems;
        }

        private void ItemUpdatedAgeRatings(object sender, ItemUpdatedEventArgs<AgeRating> args)
        {
            if (!Settings.Settings.RenameMergeRules)
            {
                return;
            }

            if (args.UpdatedItems
                .Where(item => item.OldData.Name != item.NewData.Name && !string.IsNullOrEmpty(item.OldData.Name))
                .ToList().Aggregate(false,
                    (current, item)
                        => current | Settings.Settings.MergeRules.FindAndRenameRule(FieldType.AgeRating,
                            item.OldData.Name, item.NewData.Name)))
            {
                SavePluginSettings(Settings.Settings);
            }
        }

        private void ItemUpdatedCategories(object sender, ItemUpdatedEventArgs<Category> args)
        {
            if (!Settings.Settings.RenameDefaults && !Settings.Settings.RenameMergeRules)
            {
                return;
            }

            List<ItemUpdateEvent<Category>> items = args.UpdatedItems
                .Where(item => item.OldData.Name != item.NewData.Name && !string.IsNullOrEmpty(item.OldData.Name) &&
                               !string.IsNullOrEmpty(item.NewData.Name))
                .ToList();

            if (items.Count == 0)
            {
                return;
            }

            bool mustSave = false;

            if (Settings.Settings.RenameDefaults)
            {
                foreach (ItemUpdateEvent<Category> item in items)
                {
                    MetadataObject tag = Settings.Settings.DefaultCategories?.FirstOrDefault(x => x.Name == item.OldData.Name);

                    if (tag == null)
                    {
                        continue;
                    }

                    tag.Name = item.NewData.Name;
                    mustSave = true;
                }
            }

            if (Settings.Settings.RenameMergeRules)
            {
                mustSave = items.Aggregate(mustSave,
                    (current, item)
                        => current |
                           Settings.Settings.MergeRules.FindAndRenameRule(FieldType.Category, item.OldData.Name,
                               item.NewData.Name));
            }

            if (mustSave)
            {
                SavePluginSettings(Settings.Settings);
            }
        }

        private void ItemUpdatedFeatures(object sender, ItemUpdatedEventArgs<GameFeature> args)
        {
            if (!Settings.Settings.RenameMergeRules)
            {
                return;
            }

            if (args.UpdatedItems
                .Where(item => item.OldData.Name != item.NewData.Name && !string.IsNullOrEmpty(item.OldData.Name))
                .ToList().Aggregate(false,
                    (current, item)
                        => current | Settings.Settings.MergeRules.FindAndRenameRule(FieldType.Feature,
                            item.OldData.Name, item.NewData.Name)))
            {
                SavePluginSettings(Settings.Settings);
            }
        }

        private void ItemUpdatedGenres(object sender, ItemUpdatedEventArgs<Genre> args)
        {
            if (!Settings.Settings.RenameMergeRules)
            {
                return;
            }

            if (args.UpdatedItems
                .Where(item => item.OldData.Name != item.NewData.Name && !string.IsNullOrEmpty(item.OldData.Name))
                .ToList().Aggregate(false,
                    (current, item)
                        => current | Settings.Settings.MergeRules.FindAndRenameRule(FieldType.Genre,
                            item.OldData.Name, item.NewData.Name)))
            {
                SavePluginSettings(Settings.Settings);
            }
        }

        private void ItemUpdatedSeries(object sender, ItemUpdatedEventArgs<Series> args)
        {
            if (!Settings.Settings.RenameMergeRules)
            {
                return;
            }

            if (args.UpdatedItems
                .Where(item => item.OldData.Name != item.NewData.Name && !string.IsNullOrEmpty(item.OldData.Name))
                .ToList().Aggregate(false,
                    (current, item)
                        => current | Settings.Settings.MergeRules.FindAndRenameRule(FieldType.Series,
                            item.OldData.Name, item.NewData.Name)))
            {
                SavePluginSettings(Settings.Settings);
            }
        }

        private void ItemUpdatedTags(object sender, ItemUpdatedEventArgs<Tag> args)
        {
            if (!Settings.Settings.RenameDefaults && !Settings.Settings.RenameMergeRules)
            {
                return;
            }

            List<ItemUpdateEvent<Tag>> items = args.UpdatedItems
                .Where(item => item.OldData.Name != item.NewData.Name && !string.IsNullOrEmpty(item.OldData.Name) &&
                               !string.IsNullOrEmpty(item.NewData.Name))
                .ToList();

            if (items.Count == 0)
            {
                return;
            }

            bool mustSave = false;

            if (Settings.Settings.RenameDefaults)
            {
                foreach (ItemUpdateEvent<Tag> item in items)
                {
                    MetadataObject tag = Settings.Settings.DefaultTags?.FirstOrDefault(x => x.Name == item.OldData.Name);

                    if (tag == null)
                    {
                        continue;
                    }

                    tag.Name = item.NewData.Name;
                    mustSave = true;
                }
            }

            if (Settings.Settings.RenameMergeRules)
            {
                mustSave = items.Aggregate(mustSave,
                    (current, item)
                        => current |
                           Settings.Settings.MergeRules.FindAndRenameRule(FieldType.Tag, item.OldData.Name,
                               item.NewData.Name));
            }

            if (mustSave)
            {
                SavePluginSettings(Settings.Settings);
            }
        }

        private void ShowEditor() => ShowEditor(null);

        private void ShowEditor(List<Game> games)
        {
            Log.Debug("=== ShowEditor: Start ===");
            DateTime ts = DateTime.Now;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                MetadataObjects metadataObjects = new MetadataObjects(Settings.Settings);
                string windowTitle = "LOCMetadataUtilitiesEditor";

                if (games != null)
                {
                    metadataObjects.LoadGameMetadata(games);
                    windowTitle = "LOCMetadataUtilitiesEditorForGames";
                }
                else
                {
                    metadataObjects.LoadMetadata();
                }

                MetadataEditorViewModel viewModel = new MetadataEditorViewModel(this, metadataObjects);

                MetadataEditorView editorView = new MetadataEditorView();

                Window window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString(windowTitle), Settings.Settings.EditorWindowWidth, Settings.Settings.EditorWindowHeight);
                window.Content = editorView;
                window.DataContext = viewModel;

                Log.Debug($"=== ShowEditor: Show Dialog ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");

                window.ShowDialog();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing Metadata Editor", true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ShowSettings() => OpenSettingsView();
    }
}