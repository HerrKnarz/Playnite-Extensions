using KNARZhelper;
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
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var menuSection = ResourceProvider.GetString("LOCScreenshotUtilitiesName");

            var menuItems = new List<GameMenuItem>();

            menuItems.AddRange(new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = "Add screenshots from Steam",
                    MenuSection = menuSection,
                    Icon = "suShowScreenshotsIcon",
                    Action = a => GetScreenshots(args.Games.FirstOrDefault())
                }
            });

            return menuItems;
        }

        private void GetScreenshots(Game game)
        {
            var steamId = SteamHelper.GetSteamId(game);

            if (string.IsNullOrEmpty(steamId))
            {
                PlayniteApi.Dialogs.ShowMessage("Could not determine Steam ID for the selected game.", "Screenshot Utilities");
                return;
            }

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
            // Add code to be executed when Playnite is initialized.
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