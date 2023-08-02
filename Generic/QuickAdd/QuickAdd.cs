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

        private static bool AddFeature(Game game, Guid featureId)
        {
            List<Guid> featureIds = game.FeatureIds ?? (game.FeatureIds = new List<Guid>());

            bool added = featureIds.AddMissing(featureId);
            API.Instance.Database.Games.Update(game);

            return added;
        }

        private void DoForAll(List<Game> games, Guid featureId, bool showDialog = false)
        {
            if (games.Count == 1)
            {
                AddFeature(games.First(), featureId);
            }
            // if we have more than one game in the list, we want to start buffered mode and show a progress bar.
            else if (games.Count > 1)
            {
                int gamesAffected = 0;

                using (PlayniteApi.Database.BufferedUpdate())
                {
                    GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                        $"{ResourceProvider.GetString("LOCQuickAddName")} - {ResourceProvider.GetString("LOCQuickAddProgressFeatures")}",
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
                                activateGlobalProgress.Text = $"{ResourceProvider.GetString("LOCQuickAddName")}{Environment.NewLine}{ResourceProvider.GetString("LOCQuickAddProgressFeatures")}{Environment.NewLine}{game.Name}";

                                if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                if (AddFeature(game, featureId))
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
                    PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCQuickAddFeaturesAdded"), gamesAffected));
                }
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            string menuSection = ResourceProvider.GetString("LOCQuickAddName");
            string featuresLabel = ResourceProvider.GetString("LOCFeaturesLabel");

            List<Game> games = args.Games.Distinct().ToList();

            return API.Instance.Database.Features.Select(feature => new GameMenuItem
            {
                Description = feature.Name,
                MenuSection = $"{menuSection}|{featuresLabel}",
                Action = a => DoForAll(games, feature.Id, true)
            }).ToList();
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new QuickAddSettingsView();
    }
}