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
using KNARZhelper.DatabaseObjectTypes;
using UserControl = System.Windows.Controls.UserControl;

namespace MetadataUtilities
{
    public class MetadataUtilities : GenericPlugin
    {
        private readonly List<IEditableObjectType> _fieldTypes = FieldTypeHelper.GetAllTypes<IEditableObjectType>(true).ToList();

        public MetadataUtilities(IPlayniteAPI api) : base(api)
        {
            Settings = new SettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            IsUpdating = false;

            foreach (IEditableObjectType type in _fieldTypes)
            {
                type.RenameObject += OnRenameObject;
            }

            api.Database.Games.ItemUpdated += Games_ItemUpdated;

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
            if (IsUpdating)
            {
                return;
            }

            // Only run for games, that have values in one of the supported fields and those differ
            // from the ones before.
            List<Game> games = args.UpdatedItems.Where(item =>
                item.OldData == null ||
                (item.NewData.AgeRatingIds != null &&
                 (item.OldData.AgeRatingIds == null ||
                  !new HashSet<Guid>(item.OldData.AgeRatingIds).SetEquals(item.NewData.AgeRatingIds))) ||
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

            if (games.Count > 0)
            {
                if (Settings.Settings.RemoveUnwantedOnMetadataUpdate)
                {
                    MetadataFunctions.DoForAll(this, games, RemoveUnwantedAction.Instance(this));
                }

                if (Settings.Settings.MergeMetadataOnMetadataUpdate)
                {
                    MetadataFunctions.MergeItems(this, games);
                }
            }

            // Execute conditional actions for all games, since those take nearly every possible
            // field into account
            MetadataFunctions.DoForAll(this, args.UpdatedItems.Select(item => item.NewData).ToList(), ExecuteConditionalActionsAction.Instance(this));
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
                    MetadataFunctions.DoForAll(this, games, SetUserScoreAction.Instance(this), true, ActionModifierTypes.None, 0)
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
                        MetadataFunctions.DoForAll(this, games, SetUserScoreAction.Instance(this), true, ActionModifierTypes.None, rating)
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
                    Action = a => MetadataFunctions.DoForAll(this, games, AddDefaultsAction.Instance(this), true)
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuRemoveUnwanted"),
                    MenuSection = menuSection,
                    Icon = "muRemoveIcon",
                    Action = a => MetadataFunctions.DoForAll(this, games, RemoveUnwantedAction.Instance(this), true)
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuMergeMetadata"),
                    MenuSection = menuSection,
                    Icon = "muMergeIcon",
                    Action = a => MetadataFunctions.MergeItems(this, games, null, true)
                }
            });

            menuItems.AddRange(Settings.Settings.MergeRules.OrderBy(x => x.TypeAndName).Select(rule => new GameMenuItem
            {
                Description = rule.TypeAndName,
                MenuSection = $"{menuSection}|{mergeSection}",
                Action = a => MetadataFunctions.MergeItems(this, games, rule)
            }));

            menuItems.AddRange(Settings.Settings.ConditionalActions.OrderBy(x => x.Name).Select(action => new GameMenuItem
            {
                Description = action.Name,
                MenuSection = $"{menuSection}|{conditionalSection}",
                Action = a => MetadataFunctions.DoForAll(this, games, ExecuteConditionalActionsAction.Instance(this), true,
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

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            base.OnApplicationStarted(args);

            if (!Settings.Settings.RemoveUnusedOnStartup)
            {
                return;
            }

            RemoveUnused();
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            List<Game> games = PlayniteApi.Database.Games
                .Where(x => x.Added != null && x.Added > Settings.Settings.LastAutoLibUpdate).ToList();

            MetadataFunctions.DoForAll(this, games, AddDefaultsAction.Instance(this));

            Settings.Settings.LastAutoLibUpdate = DateTime.Now;

            SavePluginSettings(Settings.Settings);
        }

        public void RemoveUnused()
        {
            Cursor.Current = Cursors.WaitCursor;
            IsUpdating = true;

            try
            {
                MetadataFunctions.RemoveUnusedMetadata(Settings.Settings, true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                IsUpdating = false;
            }
        }

        private IEnumerable<GameMenuItem> CreateMenuItems(string baseMenu, List<Game> games, IReadOnlyCollection<QuickAddObject> dbObjects, ActionModifierTypes action = ActionModifierTypes.Add)
        {
            List<GameMenuItem> menuItems = new List<GameMenuItem>();

            if (dbObjects.Count == 0)
            {
                return menuItems;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (QuickAddObject dbObject in dbObjects
                         .Where(x => (action == ActionModifierTypes.Add && x.Add) ||
                                     (action == ActionModifierTypes.Remove && x.Remove) ||
                                     (action == ActionModifierTypes.Toggle && x.Toggle)))
            {
                string customMenu = dbObject.CustomPath?.Trim().Length > 0
                    ? dbObject.CustomPath.Replace("{type}", dbObject.Type.ToString()).Replace("{action}", action.ToString())
                    : Settings.Settings.QuickAddCustomPath?.Trim().Length > 0
                        ? Settings.Settings.QuickAddCustomPath.Replace("{type}", dbObject.Type.ToString()).Replace("{action}", action.ToString())
                        : string.Format(ResourceProvider.GetString($"LOCMetadataUtilitiesMenuQuickAdd{action}"), ResourceProvider.GetString($"LOC{dbObject.Type}Label"));

                int checkedCount = 0;

                if (dbObject.Type.GetTypeManager() is IObjectType type)
                {
                    checkedCount = type.GetGameCount(games, dbObject.Id);
                }

                menuItems.Add(new GameMenuItem
                {
                    Icon = checkedCount == games.Count ? "muAllCheckedIcon" : checkedCount > 0 ? "muSomeCheckedIcon" : "",
                    Description = dbObject.Name,
                    MenuSection = baseMenu + customMenu,
                    Action = a => MetadataFunctions.DoForAll(this, games, QuickAddAction.Instance(this), true, action, dbObject)
                });
            }

            return menuItems;
        }

        private void OnRenameObject(object sender, string oldName, string newName) => MetadataFunctions.RenameObject(this, (IFieldType)sender, oldName, newName);

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