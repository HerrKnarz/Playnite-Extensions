using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using MetadataUtilities.Actions;
using MetadataUtilities.Enums;
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
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using MergeAction = MetadataUtilities.Actions.MergeAction;
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

            foreach (var type in _fieldTypes)
            {
                type.RenameObject += OnRenameObject;
            }

            api.Database.Games.ItemUpdated += Games_ItemUpdated;

            var iconResourcesToAdd = new Dictionary<string, string>
            {
                { "muEditorIcon", "\xf005" },
                { "muMergeIcon", "\xef29" },
                { "muRemoveIcon", "\xee09" },
                { "muTagIcon", "\xf004" },
                { "muAllCheckedIcon", "\xeed8" },
                { "muSomeCheckedIcon", "\xeed7" }
            };

            foreach (var iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }
        }

        public override Guid Id { get; } = Guid.Parse("485ab5f0-bfb1-4c17-93cc-20d8338673be");

        internal bool IsUpdating { get; set; }

        public SettingsViewModel Settings { get; }

        public void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            if (Settings.Settings.WriteDebugLog)
            {
                Log.Debug($"IsUpdating: {IsUpdating}");
            }

            if (IsUpdating)
            {
                return;
            }

            // some actions only run for games, that have values in one of the supported fields and
            // those differ from the ones before.
            var games = args.UpdatedItems.Where(item =>
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
                  !new HashSet<Guid>(item.OldData.TagIds).SetEquals(item.NewData.TagIds))))
                .Select(item => item.NewData.Id).Distinct().ToHashSet();

            var myGames = args.UpdatedItems.Select(x => new MyGame()
            {
                Game = x.NewData,
                ExecuteConditionalActions = true,
                ExecuteMergeRules = games.Contains(x.NewData.Id),
                ExecuteRemoveUnwanted = games.Contains(x.NewData.Id)
            }).ToList();

            MetadataFunctions.DoForAll(this, myGames, AfterMetadataUpdateAction.Instance(Settings.Settings), false, ActionModifierType.IsCombi);
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var menuSection = ResourceProvider.GetString("LOCMetadataUtilitiesName");
            var mergeSection = ResourceProvider.GetString("LOCMetadataUtilitiesSettingsMergeRules");
            var conditionalSection = ResourceProvider.GetString("LOCMetadataUtilitiesSettingsTabConditionalActions");
            var menuItems = new List<GameMenuItem>();
            var games = args.Games.Distinct().ToList();
            var myGames = games.Select(x => new MyGame() { Game = x }).ToList();

            var item = new GameMenuItem
            {
                Description = "",
                MenuSection = ResourceProvider.GetString("LOCUserScore"),
                Action = a =>
                    MetadataFunctions.DoForAll(this, myGames, SetUserScoreAction.Instance(Settings.Settings), true, ActionModifierType.None, 0)
            };
            menuItems.Add(item);

            for (var i = 1; i <= 10; i++)
            {
                var rating = i * 10;
                var menuItem = new GameMenuItem
                {
                    Description = new string('\u2605', i),
                    MenuSection = ResourceProvider.GetString("LOCUserScore"),
                    Action = a =>
                        MetadataFunctions.DoForAll(this, myGames, SetUserScoreAction.Instance(Settings.Settings), true, ActionModifierType.None, rating)
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
                    Action = a => MetadataEditorViewModel.ShowEditor(this, games)
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuAddDefaults"),
                    MenuSection = menuSection,
                    Icon = "muTagIcon",
                    Action = a => MetadataFunctions.DoForAll(this, myGames, AddDefaultsAction.Instance(Settings.Settings), true)
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuRemoveUnwanted"),
                    MenuSection = menuSection,
                    Icon = "muRemoveIcon",
                    Action = a => MetadataFunctions.DoForAll(this, myGames, RemoveUnwantedAction.Instance(Settings.Settings), true)
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuMergeMetadata"),
                    MenuSection = menuSection,
                    Icon = "muMergeIcon",
                    Action = a => MetadataFunctions.DoForAll(this, myGames, MergeAction.Instance(Settings.Settings), true)
                }
            });

            menuItems.AddRange(Settings.Settings.MergeRules.OrderBy(x => x.TypeAndName).Select(rule => new GameMenuItem
            {
                Description = rule.TypeAndName,
                MenuSection = $"{menuSection}|{mergeSection}",
                Action = a => MetadataFunctions.DoForAll(this, myGames, MergeAction.Instance(Settings.Settings), true,
                    ActionModifierType.None, rule)
            }));

            menuItems.AddRange(Settings.Settings.ConditionalActions.OrderBy(x => x.Name).Select(action => new GameMenuItem
            {
                Description = action.Name,
                MenuSection = $"{menuSection}|{conditionalSection}",
                Action = a => MetadataFunctions.DoForAll(this, myGames, ExecuteConditionalActionsAction.Instance(Settings.Settings), true,
                    ActionModifierType.IsManual, action)
            }));

            var quickAddItems = new List<GameMenuItem>();

            var baseMenu = Settings.Settings.QuickAddSingleMenuEntry
                ? ResourceProvider.GetString("LOCMetadataUtilitiesName") + "|"
                : "";

            if (Settings.Settings.QuickAddObjects.Count == 0)
            {
                return menuItems;
            }

            quickAddItems.AddRange(CreateMenuItems(baseMenu, games, Settings.Settings.QuickAddObjects));
            quickAddItems.AddRange(CreateMenuItems(baseMenu, games, Settings.Settings.QuickAddObjects, ActionModifierType.Remove));
            quickAddItems.AddRange(CreateMenuItems(baseMenu, games, Settings.Settings.QuickAddObjects, ActionModifierType.Toggle));

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
            var menuSection = ResourceProvider.GetString("LOCMetadataUtilitiesName");

            var menuItems = new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuEditor"),
                    MenuSection = $"@{menuSection}",
                    Icon = "muEditorIcon",
                    Action = a => MetadataEditorViewModel.ShowEditor(this)
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
            var games = PlayniteApi.Database.Games
                .Where(x => x.Added != null && x.Added > Settings.Settings.LastAutoLibUpdate)
                .Select(x => new MyGame() { Game = x }).ToList();

            MetadataFunctions.DoForAll(this, games, AddDefaultsAction.Instance(Settings.Settings));

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

        private IEnumerable<GameMenuItem> CreateMenuItems(string baseMenu, List<Game> games, IReadOnlyCollection<QuickAddObject> dbObjects, ActionModifierType action = ActionModifierType.Add)
        {
            var menuItems = new List<GameMenuItem>();

            if (dbObjects.Count == 0)
            {
                return menuItems;
            }

            var myGames = games.Select(x => new MyGame() { Game = x }).ToList();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var dbObject in dbObjects
                         .Where(x => (action == ActionModifierType.Add && x.Add) ||
                                     (action == ActionModifierType.Remove && x.Remove) ||
                                     (action == ActionModifierType.Toggle && x.Toggle)))
            {
                var customMenu = dbObject.CustomPath?.Trim().Length > 0
                    ? dbObject.CustomPath.Replace("{type}", dbObject.Type.ToString()).Replace("{action}", action.ToString())
                    : Settings.Settings.QuickAddCustomPath?.Trim().Length > 0
                        ? Settings.Settings.QuickAddCustomPath.Replace("{type}", dbObject.Type.ToString()).Replace("{action}", action.ToString())
                        : string.Format(ResourceProvider.GetString($"LOCMetadataUtilitiesMenuQuickAdd{action}"), ResourceProvider.GetString($"LOC{dbObject.Type}Label"));

                var checkedCount = 0;

                if (dbObject.Type.GetTypeManager() is IObjectType type)
                {
                    checkedCount = type.GetGameCount(games, dbObject.Id);
                }

                menuItems.Add(new GameMenuItem
                {
                    Icon = checkedCount == games.Count ? "muAllCheckedIcon" : checkedCount > 0 ? "muSomeCheckedIcon" : "",
                    Description = dbObject.Name,
                    MenuSection = baseMenu + customMenu,
                    Action = a => MetadataFunctions.DoForAll(this, myGames, QuickAddAction.Instance(Settings.Settings), true, action, dbObject)
                });
            }

            return menuItems;
        }

        private void OnRenameObject(object sender, string oldName, string newName) => MetadataFunctions.RenameObject(this, (IMetadataFieldType)sender, oldName, newName);

        private void ShowEditor() => MetadataEditorViewModel.ShowEditor(this);

        private void ShowSettings() => OpenSettingsView();
    }
}