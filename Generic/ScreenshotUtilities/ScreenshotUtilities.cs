using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using ScreenshotUtilities.Controls;
using ScreenshotUtilities.Models;
using ScreenshotUtilities.ViewModels;
using ScreenshotUtilities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace ScreenshotUtilities
{
    public class ScreenshotUtilities : GenericPlugin
    {
        public ScreenshotUtilitiesSettingsViewModel Settings { get; set; }

        private static readonly string _controlNameViewer = "ScreenshotViewerControl";
        private static readonly string _controlNameButton = "ButtonControl";
        private static readonly string _pluginSourceName = "ScreenshotUtilities";

        public override Guid Id { get; } = Guid.Parse("485d682f-73e9-4d54-b16f-b8dd49e88f90");

        public ScreenshotUtilities(IPlayniteAPI api) : base(api)
        {
            Settings = new ScreenshotUtilitiesSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { _controlNameViewer, _controlNameButton },
                SourceName = _pluginSourceName
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = _pluginSourceName,
                SettingsRoot = $"{nameof(Settings)}.{nameof(Settings.Settings)}"
            });

            var iconResourcesToAdd = new Dictionary<string, string>
            {
                { "suShowScreenshotsIcon", "\xef4b" },
                { "suDownloadIcon", "\xef08" }
            };

            foreach (var iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }
        }

        public void OpenScreenshotViewer(Game game)
        {
            var window = ScreenshotViewerViewModel.GetWindow(this, game);

            if (window == null)
            {
                return;
            }

            window.ShowDialog();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var menuSection = ResourceProvider.GetString("LOCScreenshotUtilitiesName");

            var menuItems = new List<GameMenuItem>();

            menuItems.AddRange(new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuShowScreenshots"),
                    MenuSection = menuSection,
                    Icon = "suShowScreenshotsIcon",
                    Action = a => OpenScreenshotViewer(args.Games.FirstOrDefault())
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuDownloadScreenshots"),
                    MenuSection = menuSection,
                    Icon = "suDownloadIcon",
                    Action = a => DownloadScreenshots(args.Games.FirstOrDefault())
                }
            });

            return menuItems;
        }

        private void DownloadScreenshots(Game game)
        {
            var groups = new ScreenshotGroups(GetPluginUserDataPath(), game.Id);

            foreach (var group in groups)
            {
                group.Download();
                group.Save();
            }
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            return args.Name == _controlNameViewer
                ? new ScreenshotViewerControl(this)
                : args.Name == _controlNameButton
                ? new ButtonControl(this)
                : (Control)null;
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

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new ScreenshotUtilitiesSettingsView();
    }
}