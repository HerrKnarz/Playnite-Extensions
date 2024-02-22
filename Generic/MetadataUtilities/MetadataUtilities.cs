using KNARZhelper;
using MetadataUtilities.Actions;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
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
            api.Database.Categories.ItemUpdated += Categories_ItemUpdated;
            api.Database.Features.ItemUpdated += Features_ItemUpdated;
            api.Database.Genres.ItemUpdated += Genres_ItemUpdated;
            api.Database.Series.ItemUpdated += Series_ItemUpdated;
            api.Database.Tags.ItemUpdated += Tags_ItemUpdated;

            Dictionary<string, string> iconResourcesToAdd = new Dictionary<string, string>
            {
                { "muEditorIcon", "\xf005" },
                { "muMergeIcon", "\xef29" },
                { "muRemoveIcon", "\xee09" },
                { "muTagIcon", "\xf004" }
            };

            foreach (KeyValuePair<string, string> iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }
        }

        internal bool IsUpdating { get; set; }

        public SettingsViewModel Settings { get; }

        public override Guid Id { get; } = Guid.Parse("485ab5f0-bfb1-4c17-93cc-20d8338673be");

        private void Tags_ItemUpdated(object sender, ItemUpdatedEventArgs<Tag> args)
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

        private void Series_ItemUpdated(object sender, ItemUpdatedEventArgs<Series> args)
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

        private void Genres_ItemUpdated(object sender, ItemUpdatedEventArgs<Genre> args)
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

        private void Features_ItemUpdated(object sender, ItemUpdatedEventArgs<GameFeature> args)
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

        private void Categories_ItemUpdated(object sender, ItemUpdatedEventArgs<Category> args)
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

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            List<Game> games = PlayniteApi.Database.Games
                .Where(x => x.Added != null && x.Added > Settings.Settings.LastAutoLibUpdate).ToList();

            DoForAll(games, AddDefaultsAction.Instance(this));

            Settings.Settings.LastAutoLibUpdate = DateTime.Now;

            SavePluginSettings(Settings.Settings);
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
                MetadataObjects.RemoveUnusedMetadata(Settings.Settings, true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            if (!Settings.Settings.MergeMetadataOnMetadataUpdate || IsUpdating)
            {
                return;
            }

            // Only run for games, that have values in one of the supported fields and those differ from the ones before.
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
            if (!showDialog || games?.Count() == 1)
            {
                return;
            }

            Cursor.Current = Cursors.Default;
            PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogMergedMetadataMessage"), gamesAffected.Distinct().Count()));
        }

        public void RemoveUnused()
        {
            Cursor.Current = Cursors.WaitCursor;
            IsUpdating = true;

            try
            {
                MetadataObjects.RemoveUnusedMetadata(Settings.Settings);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                IsUpdating = false;
            }
        }

        /// <summary>
        ///     Executes a specific action for all games in a list. Shows a progress bar and result dialog and uses buffered update
        ///     mode if the list contains more than one game.
        /// </summary>
        /// <param name="games">List of games to be processed</param>
        /// <param name="action">Instance of the action to be executed</param>
        /// <param name="showDialog">If true a dialog will be shown after completion</param>
        /// <param name="actionModifier">specifies the type of action to execute, if more than one is possible.</param>
        internal void DoForAll(List<Game> games, IBaseAction action, bool showDialog = false, ActionModifierTypes actionModifier = ActionModifierTypes.None)
        {
            IsUpdating = true;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (games.Count == 1)
                {
                    action.Execute(games.First(), actionModifier, false);
                }
                // if we have more than one game in the list, we want to start buffered mode and show a progress bar.
                else if (games.Count > 1)
                {
                    int gamesAffected = 0;

                    using (PlayniteApi.Database.BufferedUpdate())
                    {
                        if (!action.Prepare(actionModifier))
                        {
                            return;
                        }

                        GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                            $"{ResourceProvider.GetString("LOCMetadataUtilitiesName")} - {ResourceProvider.GetString(action.ProgressMessage)}",
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
                                        $"{ResourceProvider.GetString("LOCMetadataUtilitiesName")}{Environment.NewLine}{ResourceProvider.GetString(action.ProgressMessage)}{Environment.NewLine}{game.Name}";

                                    if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    if (action.Execute(game, actionModifier))
                                    {
                                        gamesAffected++;
                                    }

                                    activateGlobalProgress.CurrentProgressValue++;
                                }
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

        private void ShowEditor()
        {
            Log.Debug("=== ShowEditor: Start ===");
            DateTime ts = DateTime.Now;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                MetadataObjects metadataObjects = new MetadataObjects(Settings.Settings);
                metadataObjects.LoadMetadata();

                MetadataEditorViewModel viewModel = new MetadataEditorViewModel(this, metadataObjects);

                MetadataEditorView editorView = new MetadataEditorView();

                Window window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCMetadataUtilitiesEditor"), Settings.Settings.EditorWindowWidth, Settings.Settings.EditorWindowHeight);
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

        public override IEnumerable<TopPanelItem> GetTopPanelItems()
        {
            if (!Settings.Settings.ShowTopPanelButton)
            {
                yield break;
            }

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

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            string menuSection = ResourceProvider.GetString("LOCMetadataUtilitiesName");
            string mergeSection = ResourceProvider.GetString("LOCMetadataUtilitiesSettingsMergeRules");
            List<GameMenuItem> menuItems = new List<GameMenuItem>();
            List<Game> games = args.Games.Distinct().ToList();

            menuItems.AddRange(new List<GameMenuItem>
            {
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

            return menuItems;
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new SettingsView();
    }
}