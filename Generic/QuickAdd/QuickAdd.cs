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

    public class QuickAdd : GenericPlugin
    {
        public QuickAdd(IPlayniteAPI api) : base(api)
        {
            Settings = new QuickAddSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            API.Instance.Database.Tags.ItemCollectionChanged += (sender, args) =>
                Settings.Settings.QuickTags = QuickTags.GetTags(Settings.Settings.CheckedTags);
            API.Instance.Database.Tags.ItemUpdated += (sender, args) =>
                Settings.Settings.QuickTags = QuickTags.GetTags(Settings.Settings.CheckedTags);

            API.Instance.Database.Features.ItemCollectionChanged += (sender, args) =>
                Settings.Settings.QuickFeatures = QuickFeatures.GetFeatures(Settings.Settings.CheckedFeatures);
            API.Instance.Database.Features.ItemUpdated += (sender, args) =>
                Settings.Settings.QuickFeatures = QuickFeatures.GetFeatures(Settings.Settings.CheckedFeatures);

            API.Instance.Database.Categories.ItemCollectionChanged += (sender, args) =>
                Settings.Settings.QuickCategories = QuickCategories.GetCategories(Settings.Settings.CheckedCategories);
            API.Instance.Database.Categories.ItemUpdated += (sender, args) =>
                Settings.Settings.QuickCategories = QuickCategories.GetCategories(Settings.Settings.CheckedCategories);
        }

        private QuickAddSettingsViewModel Settings { get; }

        public override Guid Id { get; } = Guid.Parse("44def840-a5dc-4fdf-8fd7-37ffe57187d6");

        private static bool AddValue(Game game, Guid id, FieldType type)
        {
            List<Guid> ids;

            switch (type)
            {
                case FieldType.Feature:
                    ids = game.FeatureIds ?? (game.FeatureIds = new List<Guid>());

                    break;
                case FieldType.Tag:
                    ids = game.TagIds ?? (game.TagIds = new List<Guid>());

                    break;
                case FieldType.Category:
                    ids = game.CategoryIds ?? (game.CategoryIds = new List<Guid>());

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            bool added = ids.AddMissing(id);

            if (added)
            {
                API.Instance.Database.Games.Update(game);
            }

            return added;
        }

        private void DoForAll(List<Game> games, Guid id, FieldType type)
        {
            if (games.Count == 1)
            {
                AddValue(games.First(), id, type);
            }
            // if we have more than one game in the list, we want to start buffered mode and show a progress bar.
            else if (games.Count > 1)
            {
                int gamesAffected = 0;

                string progressLabel;
                string progressResult;

                switch (type)
                {
                    case FieldType.Feature:
                        progressLabel = ResourceProvider.GetString("LOCQuickAddProgressFeatures");
                        progressResult = ResourceProvider.GetString("LOCQuickAddFeaturesAdded");
                        break;
                    case FieldType.Tag:
                        progressLabel = ResourceProvider.GetString("LOCQuickAddProgressTags");
                        progressResult = ResourceProvider.GetString("LOCQuickAddTagsAdded");
                        break;
                    case FieldType.Category:
                        progressLabel = ResourceProvider.GetString("LOCQuickAddProgressCategories");
                        progressResult = ResourceProvider.GetString("LOCQuickAddCategoriesAdded");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

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

                                if (AddValue(game, id, type))
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
                PlayniteApi.Dialogs.ShowMessage(string.Format(progressResult, gamesAffected));
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            string featureLabel = ResourceProvider.GetString("LOCQuickAddAddFeature");
            string tagLabel = ResourceProvider.GetString("LOCQuickAddAddTag");
            string categoryLabel = ResourceProvider.GetString("LOCQuickAddAddCategory");

            List<Game> games = args.Games.Distinct().ToList();

            if (!Settings.Settings.QuickFeatures.Any())
            {
                Settings.Settings.QuickFeatures = QuickFeatures.GetFeatures(Settings.Settings.CheckedFeatures);
            }

            List<GameMenuItem> menuItems = new List<GameMenuItem>();

            if (Settings.Settings.QuickFeatures.Any(x => x.Checked))
            {
                menuItems.AddRange(Settings.Settings.QuickFeatures.Where(x => x.Checked).Select(feature => new GameMenuItem
                {
                    Description = feature.Name,
                    MenuSection = $"{featureLabel}",
                    Action = a => DoForAll(games, feature.Id, FieldType.Feature)
                }).OrderBy(x => x.Description));
            }

            if (!Settings.Settings.QuickTags.Any())
            {
                Settings.Settings.QuickTags = QuickTags.GetTags(Settings.Settings.CheckedTags);
            }

            if (Settings.Settings.QuickTags.Any(x => x.Checked))
            {
                menuItems.AddRange(Settings.Settings.QuickTags.Where(x => x.Checked).Select(tag => new GameMenuItem
                {
                    Description = tag.Name,
                    MenuSection = $"{tagLabel}",
                    Action = a => DoForAll(games, tag.Id, FieldType.Tag)
                }).OrderBy(x => x.Description));
            }

            if (!Settings.Settings.QuickCategories.Any())
            {
                Settings.Settings.QuickCategories = QuickCategories.GetCategories(Settings.Settings.CheckedCategories);
            }

            if (Settings.Settings.QuickCategories.Any(x => x.Checked))
            {
                menuItems.AddRange(Settings.Settings.QuickCategories.Where(x => x.Checked).Select(category => new GameMenuItem
                {
                    Description = category.Name,
                    MenuSection = $"{categoryLabel}",
                    Action = a => DoForAll(games, category.Id, FieldType.Category)
                }).OrderBy(x => x.Description));
            }

            return menuItems;
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new QuickAddSettingsView();
    }
}