﻿using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using MetadataUtilities.Actions;
using MetadataUtilities.Controls;
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
using Control = System.Windows.Controls.Control;
using MergeAction = MetadataUtilities.Actions.MergeAction;
using UserControl = System.Windows.Controls.UserControl;

namespace MetadataUtilities
{
    public class MetadataUtilities : GenericPlugin
    {
        private readonly Dictionary<string, FieldType> _controlTypes = FieldTypeHelper.ItemListFieldValues()
            .ToDictionary(type => $"{Enum.GetName(typeof(FieldType), type.Key)}PrefixItemControl", type => type.Key);

        private readonly List<IEditableObjectType> _fieldTypes = FieldTypeHelper.GetAllTypes<IEditableObjectType>(true).ToList();

        public MetadataUtilities(IPlayniteAPI api) : base(api)
        {
            ControlCenter.Instance = new ControlCenter(this);

            Settings = new SettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            foreach (IEditableObjectType type in _fieldTypes)
            {
                type.RenameObject += OnRenameObject;
            }

            api.Database.Games.ItemUpdated += Games_ItemUpdated;
            api.Database.Games.ItemCollectionChanged += Games_ItemCollectionChanged;

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = _controlTypes.Select(type => type.Key).ToList(),
                SourceName = "MetadataUtilities"
            });

            Dictionary<string, string> iconResourcesToAdd = new Dictionary<string, string>
            {
                { "muEditorIcon", "\xf005" },
                { "muMergeIcon", "\xef29" },
                { "muRemoveIcon", "\xee09" },
                { "muTagIcon", "\xf004" },
                { "muAllCheckedIcon", "\xeed8" },
                { "muSomeCheckedIcon", "\xeed7" },
                { "muUndoIcon", "\xee0b" }
            };

            foreach (KeyValuePair<string, string> iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }
        }

        public override Guid Id { get; } = Guid.Parse("485ab5f0-bfb1-4c17-93cc-20d8338673be");

        public List<PrefixItemControl> PrefixItemControls { get; set; } = new List<PrefixItemControl>();

        public SettingsViewModel Settings { get; }

        public void Games_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> args)
        {
            ControlCenter.Instance.KnownGames.RemoveWhere(x => args.RemovedItems.Any(y => y.Id == x));
        }

        public void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            if (Settings.Settings.WriteDebugLog)
            {
                Log.Debug($"IsUpdating: {ControlCenter.Instance.IsUpdating}");
            }

            if (ControlCenter.Instance.IsUpdating)
            {
                return;
            }

            // some actions only run for games, that have values in one of the supported fields and
            // those differ from the ones before.
            HashSet<Guid> games = args.UpdatedItems.Where(item =>
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

            List<MyGame> myGames = args.UpdatedItems.Select(x => new MyGame()
            {
                Game = x.NewData,
                ExecuteConditionalActions = true,
                ExecuteMergeRules = games.Contains(x.NewData.Id),
                ExecuteRemoveUnwanted = games.Contains(x.NewData.Id)
            }).ToList();

            AfterGameUpdateAction.Instance().DoForAll(myGames, false, ActionModifierType.IsCombi);

            SavePluginSettings(Settings.Settings);

            API.Instance.MainView.UIDispatcher.Invoke(delegate
            {
                foreach (PrefixItemControl control in PrefixItemControls)
                {
                    control.RefreshData();
                }
            });
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            string menuSection = ResourceProvider.GetString("LOCMetadataUtilitiesName");
            string mergeSection = ResourceProvider.GetString("LOCMetadataUtilitiesSettingsMergeRules");
            string conditionalSection = ResourceProvider.GetString("LOCMetadataUtilitiesSettingsTabConditionalActions");
            List<GameMenuItem> menuItems = new List<GameMenuItem>();
            List<Game> games = args.Games.Distinct().ToList();
            List<MyGame> myGames = games.Select(x => new MyGame() { Game = x }).ToList();

            GameMenuItem item = new GameMenuItem
            {
                Description = "",
                MenuSection = ResourceProvider.GetString("LOCUserScore"),
                Action = a =>
                    SetUserScoreAction.Instance().DoForAll(myGames, true, ActionModifierType.None, 0)
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
                        SetUserScoreAction.Instance().DoForAll(myGames, true, ActionModifierType.None, rating)
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
                    Action = a => MetadataEditorViewModel.ShowEditor(games)
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuRemoveUnwanted"),
                    MenuSection = menuSection,
                    Icon = "muRemoveIcon",
                    Action = a => RemoveUnwantedAction.Instance().DoForAll(myGames, true)
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuMergeMetadata"),
                    MenuSection = menuSection,
                    Icon = "muMergeIcon",
                    Action = a => MergeAction.Instance().DoForAll(myGames, true)
                }
            });

            /*menuItems.AddRange(Settings.Settings.MergeRules.OrderBy(x => x.TypeAndName).Select(rule => new GameMenuItem
            {
                Description = rule.TypeAndName,
                MenuSection = $"{menuSection}|{mergeSection}",
                Action = a => MergeAction.Instance().DoForAll(myGames, true,
                    ActionModifierType.None, rule)
            })); */

            menuItems.AddRange(Settings.Settings.ConditionalActions.OrderBy(x => x.Name).Select(action => new GameMenuItem
            {
                Description = action.Name,
                MenuSection = $"{menuSection}|{conditionalSection}",
                Action = a => ExecuteConditionalActionsAction.Instance().DoForAll(myGames, true,
                    ActionModifierType.IsManual, action)
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

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (!_controlTypes.TryGetValue(args.Name, out FieldType type))
            {
                return null;
            }

            PrefixItemControl newControl = new PrefixItemControl(type);

            PrefixItemControls.Add(newControl);

            return newControl;

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
                    Action = a => MetadataEditorViewModel.ShowEditor()
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
                },
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCMetadataUtilitiesMenuResetNewGames"),
                    MenuSection = $"@{menuSection}",
                    Icon = "muUndoIcon",
                    Action = a => ControlCenter.Instance.ResetNewGames()
                }
            };

            return menuItems;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SettingsView();
        }

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

            ControlCenter.Instance.GetKnownGames();

            if (!Settings.Settings.TypeConfigs.Any(x => x.RemoveUnusedItems))
            {
                return;
            }

            RemoveUnused();
        }

        public void RemoveUnused()
        {
            Cursor.Current = Cursors.WaitCursor;
            ControlCenter.Instance.IsUpdating = true;

            try
            {
                ControlCenter.Instance.RemoveUnusedMetadata(true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                ControlCenter.Instance.IsUpdating = false;
            }
        }

        private static bool OnRenameObject(object sender, string oldName, string newName)
        {
            return ControlCenter.Instance.RenameObject((IMetadataFieldType)sender, oldName, newName);
        }

        private static void ShowEditor()
        {
            MetadataEditorViewModel.ShowEditor();
        }

        private IEnumerable<GameMenuItem> CreateMenuItems(string baseMenu, List<Game> games, IReadOnlyCollection<QuickAddObject> dbObjects, ActionModifierType action = ActionModifierType.Add)
        {
            List<GameMenuItem> menuItems = new List<GameMenuItem>();

            if (dbObjects.Count == 0)
            {
                return menuItems;
            }

            List<MyGame> myGames = games.Select(x => new MyGame() { Game = x }).ToList();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (QuickAddObject dbObject in dbObjects
                         .Where(x => (action == ActionModifierType.Add && x.Add) ||
                                     (action == ActionModifierType.Remove && x.Remove) ||
                                     (action == ActionModifierType.Toggle && x.Toggle)))
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
                    Action = a => QuickAddAction.Instance().DoForAll(myGames, true, action, dbObject)
                });
            }

            return menuItems;
        }

        private void ShowSettings()
        {
            OpenSettingsView();
        }
    }
}