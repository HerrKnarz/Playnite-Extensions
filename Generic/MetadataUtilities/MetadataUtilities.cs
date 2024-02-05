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
using System.Windows.Media;

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

            if (Settings.Settings.RemoveUnusedOnStartup)
            {
                MetadataListObjects.RemoveUnusedMetadata(true, Settings.Settings.IgnoreHiddenGamesInRemoveUnused);
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

            DoForAll(games, MergeAction.Instance(this));
        }

        /// <summary>
        ///     Executes a specific action for all games in a list. Shows a progress bar and result dialog and uses buffered update
        ///     mode if the list contains more than one game.
        /// </summary>
        /// <param name="games">List of games to be processed</param>
        /// <param name="action">Instance of the action to be executed</param>
        /// <param name="showDialog">If true a dialog will be shown after completion</param>
        /// <param name="actionModifier">specifies the type of action to execute, if more than one is possible.</param>
        private void DoForAll(List<Game> games, BaseAction action, bool showDialog = false, ActionModifierTypes actionModifier = ActionModifierTypes.None)
        {
            IsUpdating = true;

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
                    if (showDialog)
                    {
                        PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString(action.ResultMessage), gamesAffected));
                    }
                }
            }
            finally
            {
                IsUpdating = false;
            }
        }

        private void ShowEditor()
        {
            try
            {
                MetadataListObjects metadataListObjects = new MetadataListObjects(Settings.Settings);
                metadataListObjects.LoadMetadata();

                MetadataEditorViewModel viewModel = new MetadataEditorViewModel(this, metadataListObjects);

                MetadataEditorView editorView = new MetadataEditorView();

                Window window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCMetadataUtilitiesEditor"), Settings.Settings.EditorWindowWidth, Settings.Settings.EditorWindowHeight);
                window.Content = editorView;
                window.DataContext = viewModel;
                window.ShowDialog();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing Metadata Editor", true);
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
                    Action = a => MetadataListObjects.RemoveUnusedMetadata(false, Settings.Settings.IgnoreHiddenGamesInRemoveUnused)
                }
            };

            return menuItems;
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            string menuSection = ResourceProvider.GetString("LOCMetadataUtilitiesName");
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
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuMergeMetadata"),
                    MenuSection = menuSection,
                    Icon = "muMergeIcon",
                    Action = a => DoForAll(games, MergeAction.Instance(this), true)
                }
            });

            return menuItems;
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new SettingsView();
    }
}