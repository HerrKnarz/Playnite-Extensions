using KNARZhelper;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace ScreenshotUtilitiesSteamProvider
{
    public class ScreenshotUtilitiesSteamProvider : GenericPlugin
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

            var iconResourcesToAdd = new Dictionary<string, string>
            {
                { "suhpSteamIcon", "\xed71" }
            };

            foreach (var iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var menuItems = new List<GameMenuItem>();

            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                return menuItems;
            }

            var menuSection = ResourceProvider.GetString("LOCScreenshotUtilitiesName");

            menuItems.AddRange(new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = "Add screenshots from Steam",
                    MenuSection = menuSection,
                    Icon = "suhpSteamIcon",
                    Action = a => GetScreenshots(args.Games.FirstOrDefault())
                }
            });

            return menuItems;
        }

        private void GetScreenshots(Game game)
        {
            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                return;
            }

            var steamId = SteamHelper.GetSteamId(game);

            if (string.IsNullOrEmpty(steamId))
            {
                return;
            }

            var apiUrl = $"http://store.steampowered.com/api/appdetails?appids={steamId}";

            var result = ApiHelper.GetJsonFromApi<SteamAppDetails>(apiUrl, "Steam");

            if ((result is null) || (result[steamId].Data.Screenshots is null) || (result[steamId].Data.Screenshots?.Count == 0))
            {
                return;
            }

            var screenshots = new RangeObservableCollection<KNARZhelper.ScreenshotsCommon.Models.Screenshot>();

            screenshots.AddRange(result[steamId].Data.Screenshots.Select(s =>
                new KNARZhelper.ScreenshotsCommon.Models.Screenshot(s.PathFull)
                {
                    ThumbnailPath = s.PathThumbnail,
                    SortOrder = s.Id
                }));

            var screenshotGroup = new ScreenshotGroup("Steam")
            {
                Provider = new ScreenshotProvider("Steam", Id),
                Screenshots = screenshots
            };

            ScreenshotHelper.SaveScreenshotGroupJson(game, screenshotGroup);
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                var notificationMessage = new NotificationMessage("Screenshot Utilities Steam Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings) => settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new ScreenshotUtilitiesSteamProviderSettingsView();
    }
}