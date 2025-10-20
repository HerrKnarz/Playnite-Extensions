using KNARZhelper;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace ScreenshotUtilitiesSteamProvider
{
    public class ScreenshotUtilitiesSteamProvider : GenericPlugin, IScreenshotProvider
    {
        private ScreenshotUtilitiesSteamProviderSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("074c1cc0-a3ec-4ea2-a136-b6a01fbf0fae");

        public ScreenshotUtilitiesSteamProvider(IPlayniteAPI api) : base(api)
        {
            settings = new ScreenshotUtilitiesSteamProviderSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public bool GetScreenshots(Game game)
        {
            try
            {
                if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled || game == null)
                {
                    return false;
                }

                var steamId = SteamHelper.GetSteamId(game);

                if (string.IsNullOrEmpty(steamId))
                {
                    return false;
                }

                var apiUrl = $"https://store.steampowered.com/api/appdetails?appids={steamId}";

                var result = ApiHelper.GetJsonFromApi<SteamAppDetails>(apiUrl, "Steam");

                if ((result is null) || (result[steamId].Data.Screenshots is null) || (result[steamId].Data.Screenshots?.Count == 0))
                {
                    return false;
                }

                var fileName = ScreenshotHelper.GenerateFileName(game.Id, Id, Id);

                var screenshotGroup = ScreenshotGroup.CreateFromFile(new FileInfo(fileName))
                    ?? new ScreenshotGroup("Steam", Id)
                    {
                        Provider = new ScreenshotProvider("Steam", Id),
                        Screenshots = new RangeObservableCollection<KNARZhelper.ScreenshotsCommon.Models.Screenshot>()
                    };

                screenshotGroup.Screenshots
                    .AddRange(result[steamId].Data.Screenshots
                    .Where(s => !screenshotGroup.Screenshots.Any(es => es.Path.Equals(s.PathFull)))
                    .Select(s =>
                   new KNARZhelper.ScreenshotsCommon.Models.Screenshot(s.PathFull)
                   {
                       ThumbnailPath = s.PathThumbnail,
                       SortOrder = s.Id
                   }));

                ScreenshotHelper.SaveScreenshotGroupJson(game, screenshotGroup);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error fetching screenshots for {game.Name}");
                return false;
            }
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                var notificationMessage = new NotificationMessage("Screenshot Utilities Steam Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);
            }
        }

        public override ISettings GetSettings(bool firstRunSettings) => settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new ScreenshotUtilitiesSteamProviderSettingsView();
    }
}