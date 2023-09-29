using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using QuickAdd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace QuickAdd
{
    public enum FieldType
    {
        Feature,
        Tag,
        Category
    }

    public enum ActionType
    {
        Add,
        Remove,
        Toggle
    }

    public class QuickAdd : GenericPlugin
    {
        public QuickAdd(IPlayniteAPI api) : base(api)
        {
            Settings = new QuickAddSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            API.Instance.Database.Categories.ItemCollectionChanged += (sender, args) =>
                Settings.RefreshList(FieldType.Category);
            API.Instance.Database.Categories.ItemUpdated += (sender, args) =>
                Settings.RefreshList(FieldType.Category);

            API.Instance.Database.Features.ItemCollectionChanged += (sender, args) =>
                Settings.RefreshList(FieldType.Feature);
            API.Instance.Database.Features.ItemUpdated += (sender, args) =>
                Settings.RefreshList(FieldType.Feature);

            API.Instance.Database.Tags.ItemCollectionChanged += (sender, args) =>
                Settings.RefreshList(FieldType.Tag);
            API.Instance.Database.Tags.ItemUpdated += (sender, args) =>
                Settings.RefreshList(FieldType.Tag);

            Dictionary<string, string> iconResourcesToAdd = new Dictionary<string, string>
            {
                { "qaAllCheckedIcon", "\xeed7" },
                { "qaSomeCheckedIcon", "\xeed8" },
                { "qaNoneCheckedIcon", "\xeedd" }
            };

            foreach (KeyValuePair<string, string> iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }
        }

        private QuickAddSettingsViewModel Settings { get; }

        public override Guid Id { get; } = Guid.Parse("44def840-a5dc-4fdf-8fd7-37ffe57187d6");

        private static bool SetFieldValue(Game game, Guid id, FieldType type, ActionType action = ActionType.Add)
        {
            List<Guid> ids;

            switch (type)
            {
                case FieldType.Category:
                    ids = game.CategoryIds ?? (game.CategoryIds = new List<Guid>());

                    break;
                case FieldType.Feature:
                    ids = game.FeatureIds ?? (game.FeatureIds = new List<Guid>());

                    break;
                case FieldType.Tag:
                    ids = game.TagIds ?? (game.TagIds = new List<Guid>());

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            bool valueSet;

            switch (action)
            {
                case ActionType.Add:
                    valueSet = ids.AddMissing(id);
                    break;
                case ActionType.Remove:
                    valueSet = ids.Remove(id);
                    break;
                case ActionType.Toggle:
                    valueSet = ids.Contains(id) ? ids.Remove(id) : ids.AddMissing(id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            if (valueSet)
            {
                API.Instance.Database.Games.Update(game);
            }

            return valueSet;
        }

        private void DoForAll(List<Game> games, Guid id, FieldType type, ActionType action = ActionType.Add)
        {
            if (games.Count == 1)
            {
                SetFieldValue(games.First(), id, type, action);
            }
            // if we have more than one game in the list, we want to start buffered mode and show a progress bar.
            else if (games.Count > 1)
            {
                int gamesAffected = 0;

                string progressLabel = string.Format(ResourceProvider.GetString($"LOCQuickAddProgress{action}"), ResourceProvider.GetString($"LOC{type}Label"));

                using (PlayniteApi.Database.BufferedUpdate())
                {
                    GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                        $"{ResourceProvider.GetString("LOCQuickAddName")} - {progressLabel}",
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
                                activateGlobalProgress.Text = $"{ResourceProvider.GetString("LOCQuickAddName")}{Environment.NewLine}{progressLabel}{Environment.NewLine}{game.Name}";

                                if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                if (SetFieldValue(game, id, type, action))
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
                PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString($"LOCQuickAddSuccess{action}"), ResourceProvider.GetString($"LOC{type}Label"), gamesAffected));
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            List<Game> games = args.Games.Distinct().ToList();

            List<GameMenuItem> menuItems = new List<GameMenuItem>();

            menuItems.AddRange(CreateMenuItems(games, Settings.Settings.QuickCategories, FieldType.Category));
            menuItems.AddRange(CreateMenuItems(games, Settings.Settings.QuickFeatures, FieldType.Feature));
            menuItems.AddRange(CreateMenuItems(games, Settings.Settings.QuickTags, FieldType.Tag));

            menuItems.AddRange(CreateMenuItems(games, Settings.Settings.QuickCategories, FieldType.Category, ActionType.Remove));
            menuItems.AddRange(CreateMenuItems(games, Settings.Settings.QuickFeatures, FieldType.Feature, ActionType.Remove));
            menuItems.AddRange(CreateMenuItems(games, Settings.Settings.QuickTags, FieldType.Tag, ActionType.Remove));

            menuItems.AddRange(CreateMenuItems(games, Settings.Settings.QuickCategories, FieldType.Category, ActionType.Toggle));
            menuItems.AddRange(CreateMenuItems(games, Settings.Settings.QuickFeatures, FieldType.Feature, ActionType.Toggle));
            menuItems.AddRange(CreateMenuItems(games, Settings.Settings.QuickTags, FieldType.Tag, ActionType.Toggle));

            return menuItems;
        }

        private IEnumerable<GameMenuItem> CreateMenuItems(List<Game> games, QuickDbObjects dbObjects, FieldType type, ActionType action = ActionType.Add)
        {
            List<GameMenuItem> menuItems = new List<GameMenuItem>();

            if (!dbObjects.Any())
            {
                Settings.RefreshList(type);
            }

            string label = string.Format(ResourceProvider.GetString($"LOCQuickAddMenu{action}"), ResourceProvider.GetString($"LOC{type}Label"));

            if (!dbObjects.Any(x => (action == ActionType.Add && x.Add) ||
                                    (action == ActionType.Remove && x.Remove) ||
                                    (action == ActionType.Toggle && x.Toggle)))
            {
                return menuItems;
            }

            foreach (QuickDBObject dbObject in dbObjects
                .Where(x => (action == ActionType.Add && x.Add) ||
                            (action == ActionType.Remove && x.Remove) ||
                            (action == ActionType.Toggle && x.Toggle)))
            {
                int checkedCount;

                switch (type)
                {
                    case FieldType.Feature:
                        checkedCount = games.Count(x => x.FeatureIds?.Contains(dbObject.Id) ?? false);
                        break;
                    case FieldType.Tag:
                        checkedCount = games.Count(x => x.TagIds?.Contains(dbObject.Id) ?? false);
                        break;
                    case FieldType.Category:
                        checkedCount = games.Count(x => x.CategoryIds?.Contains(dbObject.Id) ?? false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                menuItems.Add(new GameMenuItem
                {
                    Icon = checkedCount == 0 ? "qaNoneCheckedIcon" :
                        checkedCount < games.Count ? "qaSomeCheckedIcon" : "qaAllCheckedIcon",
                    Description = dbObject.Name,
                    MenuSection = label,
                    Action = a => DoForAll(games, dbObject.Id, type, action)
                });
            }

            return menuItems.OrderBy(x => x.Description);
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new QuickAddSettingsView();
    }
}