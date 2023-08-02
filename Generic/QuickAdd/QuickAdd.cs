using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace QuickAdd
{
    public enum FieldType
    {
        Feature,
        Tag
    }

    public class QuickAdd : GenericPlugin
    {
        public QuickAdd(IPlayniteAPI api) : base(api)
        {
            Settings = new QuickAddSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
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
            string menuSection = ResourceProvider.GetString("LOCQuickAddName");
            string featuresLabel = ResourceProvider.GetString("LOCFeaturesLabel");
            string tagLabel = ResourceProvider.GetString("LOCTagsLabel");

            List<Game> games = args.Games.Distinct().ToList();

            List<GameMenuItem> menuItems = API.Instance.Database.Features.Select(feature => new GameMenuItem
            {
                Description = feature.Name,
                MenuSection = $"{menuSection}|{featuresLabel}",
                Action = a => DoForAll(games, feature.Id, FieldType.Feature)
            }).OrderBy(x => x.Description).ToList();

            menuItems.AddRange(API.Instance.Database.Tags.Select(tag => new GameMenuItem
            {
                Description = tag.Name,
                MenuSection = $"{menuSection}|{tagLabel}",
                Action = a => DoForAll(games, tag.Id, FieldType.Tag)
            }).OrderBy(x => x.Description));

            return menuItems;
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new QuickAddSettingsView();
    }
}