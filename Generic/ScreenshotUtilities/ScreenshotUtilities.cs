using KNARZhelper;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using ScreenshotUtilities.Controls;
using ScreenshotUtilities.Models;
using ScreenshotUtilities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ScreenshotUtilities
{
    public class ScreenshotUtilities : GenericPlugin
    {
        private static readonly string _controlNameViewer = "ScreenshotViewerControl";
        private static readonly string _controlNameButton = "ButtonControl";
        private static readonly string _pluginSourceName = "ScreenshotUtilities";

        public override Guid Id { get; } = Guid.Parse("485d682f-73e9-4d54-b16f-b8dd49e88f90");

        public DispatcherTimer Timer { get; private set; }

        public List<ScreenshotProviderPlugin> ScreenshotProviders { get; set; } = new List<ScreenshotProviderPlugin>();

        public bool ProvidersInitialized { get; set; } = false;

        public List<ScreenshotViewerControl> ScreenshotViewerControls { get; set; } = new List<ScreenshotViewerControl>();

        public List<ButtonControl> ScreenshotButtonControls { get; set; } = new List<ButtonControl>();

        public ScreenshotGroups CurrentScreenshotsGroups { get; set; } = new ScreenshotGroups();

        public ScreenshotUtilitiesSettingsViewModel Settings { get; set; }

        public ScreenshotUtilities(IPlayniteAPI api) : base(api)
        {
            Settings = new ScreenshotUtilitiesSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            Timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            Timer.Tick += new EventHandler(PrepareScreenshotsEvent);

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
                { "suFetchIcon", "\xefbe" },
                { "suSearchIcon", "\xec82" }
            };

            PlayniteApi.Database.Games.ItemCollectionChanged += Games_ItemCollectionChanged;

            foreach (var iconResource in iconResourcesToAdd)
            {
                MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
            }
        }

        private void Games_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> e)
        {
            foreach (var game in e.RemovedItems)
            {
                ScreenshotHelper.RemoveScreenshots(game);
            }
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var menuSection = ResourceProvider.GetString("LOCScreenshotUtilitiesName");

            if (CurrentScreenshotsGroups != null && CurrentScreenshotsGroups.ScreenshotCount > 0)
            {
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
            }

            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuDownloadScreenshots"),
                MenuSection = menuSection,
                Icon = "suDownloadIcon",
                Action = a => DownloadScreenshotsAsync(args.Games.FirstOrDefault())
            };

            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuRefreshScreenshots"),
                MenuSection = menuSection,
                Icon = "suFetchIcon",
                Action = a => GetScreenshotsAsync(args.Games.FirstOrDefault())
            };

            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuRefreshThumbnails"),
                MenuSection = menuSection,
                Icon = "suRefreshIcon",
                Action = a => RefreshThumbnailsAsync(args.Games.FirstOrDefault())
            };

            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuResetScreenshots"),
                MenuSection = menuSection,
                Icon = "suRefreshIcon",
                Action = a => ResetScreenshotsAsync(args.Games.FirstOrDefault())
            };

            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuSearchScreenshots"),
                MenuSection = menuSection,
                Icon = "suSearchIcon",
                Action = a => SearchScreenshotsAsync(args.Games.FirstOrDefault())
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
                var buttonControl = new ButtonControl(this);

                ScreenshotButtonControls.Add(buttonControl);

                return buttonControl;
            }

            return null;
        }

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new ScreenshotUtilitiesSettingsView();

        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            if (Settings.Settings.Debug)
            {
                Log.Debug("OnGameSelected triggered!");
            }

            base.OnGameSelected(args);

            if (args.NewValue.Count == 1)
            {
                Timer.Stop();

                if (Settings.Settings.Debug)
                {
                    Log.Debug($"OnGameSelected timer stopped for game {args.NewValue[0].Name}!");
                }

                Settings.Settings.IsViewerControlVisible = false;
                CurrentScreenshotsGroups.Reset();
                RefreshControls();

                Timer.Start();

                if (Settings.Settings.Debug)
                {
                    Log.Debug($"OnGameSelected timer started for game {args.NewValue[0].Name}!");
                }
            }
        }

        private async Task DownloadScreenshotsAsync(Game game)
        {
            if (await ScreenshotActions.DownloadScreenshotsAsync(game, this))
            {
                RefreshControls();
            }
        }

        private async Task GetScreenshotsAsync(Game game)
        {
            if (await ScreenshotActions.GetScreenshotsAsync(game, this, true))
            {
                RefreshControls();
            }
        }

        internal async void PrepareScreenshotsEvent(object sender, EventArgs e)
        {
            Timer.Stop();

            var game = API.Instance.MainView.SelectedGames.FirstOrDefault();

            if (Settings.Settings.Debug)
            {
                Log.Debug($"Prepare event timer stopped for game {game.Name}!");
            }

            if (game != null)
            {
                await ScreenshotActions.PrepareScreenshotsAsync(game, this);
            }
        }

        internal void RefreshControls()
        {
            API.Instance.MainView.UIDispatcher.Invoke(delegate
            {
                foreach (var control in ScreenshotViewerControls)
                {
                    control.LoadScreenshots();
                }

                foreach (var control in ScreenshotButtonControls)
                {
                    control.LoadButton();
                }
            });
        }

        private async Task RefreshThumbnailsAsync(Game game)
        {
            if (await ScreenshotActions.RefreshThumbnailsAsync(game, this))
            {
                RefreshControls();
            }
        }

        private async Task ResetScreenshotsAsync(Game game)
        {
            if (API.Instance.Dialogs.ShowMessage(
                        ResourceProvider.GetString("LOCScreenshotUtilitiesDialogResetScreenshots"), string.Empty,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            if (await ScreenshotActions.ResetScreenshots(game, this))
            {
                RefreshControls();
            }
        }

        private async Task SearchScreenshotsAsync(Game game)
        {
            if (await ScreenshotActions.SearchScreenshotsAsync(game, this))
            {
                RefreshControls();
            }
        }
    }
}