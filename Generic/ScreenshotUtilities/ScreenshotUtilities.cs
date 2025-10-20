using KNARZhelper;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using ScreenshotUtilities.Controls;
using ScreenshotUtilities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace ScreenshotUtilities
{
    public class ScreenshotUtilities : GenericPlugin
    {
        private static readonly string _controlNameViewer = "ScreenshotViewerControl";
        private static readonly string _controlNameButton = "ButtonControl";
        private static readonly string _pluginSourceName = "ScreenshotUtilities";

        public override Guid Id { get; } = Guid.Parse("485d682f-73e9-4d54-b16f-b8dd49e88f90");

        public List<ScreenshotViewerControl> ScreenshotViewerControls { get; set; } = new List<ScreenshotViewerControl>();

        public ScreenshotGroups CurrentScreenshotsGroups { get; set; } = new ScreenshotGroups();

        public ScreenshotUtilitiesSettingsViewModel Settings { get; set; }

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
                { "suDownloadIcon", "\xef08" },
                { "suRefreshIcon", "\xefd1" },
                { "suFetchIcon", "\xefbe" }
            };

            foreach (var iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var menuSection = ResourceProvider.GetString("LOCScreenshotUtilitiesName");

            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuShowScreenshots"),
                MenuSection = menuSection,
                Icon = "suShowScreenshotsIcon",
                Action = a => ScreenshotActions.OpenScreenshotViewer(args.Games.FirstOrDefault(), this)
            };

            yield return new GameMenuItem
            {
                Description = "-",
                MenuSection = menuSection
            };

            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuDownloadScreenshots"),
                MenuSection = menuSection,
                Icon = "suDownloadIcon",
                Action = a => DownloadScreenshots(args.Games.FirstOrDefault())
            };
            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuRefreshScreenshots"),
                MenuSection = menuSection,
                Icon = "suFetchIcon",
                Action = a => GetScreenshots(args.Games.FirstOrDefault())
            };

            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuRefreshThumbnails"),
                MenuSection = menuSection,
                Icon = "suRefreshIcon",
                Action = a => RefreshThumbnails(args.Games.FirstOrDefault())
            };
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == _controlNameViewer)
            {
                var viewerControl = new ScreenshotViewerControl(this);

                ScreenshotViewerControls.Add(viewerControl);

                return viewerControl;
            }
            else if (args.Name == _controlNameButton)
            {
                return new ButtonControl(this);
            }

            return null;
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new ScreenshotUtilitiesSettingsView();

        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            base.OnGameSelected(args);

            if (args.NewValue.Count == 1)
            {
                ScreenshotActions.PrepareScreenshots(args.NewValue[0], this);

                RefreshControls();
            }
        }

        private void DownloadScreenshots(Game game)
        {
            if (ScreenshotActions.DownloadScreenshots(game, this))
            {
                RefreshControls();
            }
        }

        private void GetScreenshots(Game game)
        {
            if (ScreenshotActions.GetScreenshots(game))
            {
                RefreshControls();
            }
        }

        private void RefreshControls()
        {
            API.Instance.MainView.UIDispatcher.Invoke(delegate
            {
                foreach (var control in ScreenshotViewerControls)
                {
                    control.RefreshData();
                }
            });
        }

        private void RefreshThumbnails(Game game)
        {
            if (ScreenshotActions.RefreshThumbnails(game, this))
            {
                RefreshControls();
            }
        }
    }
}