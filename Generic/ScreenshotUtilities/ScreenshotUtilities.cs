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
        private static readonly string _controlNameButton = "ButtonControl";
        private static readonly string _controlNameViewer = "ScreenshotViewerControl";
        private static readonly string _defaultGameMenuSection = ResourceProvider.GetString("LOCScreenshotUtilitiesName");
        private static readonly string _pluginSourceName = "ScreenshotUtilities";

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
                { "suIgnoreIcon", "\xefa9" },
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

        public ScreenshotGroups CurrentScreenshotsGroups { get; set; } = new ScreenshotGroups();
        public override Guid Id { get; } = Guid.Parse("485d682f-73e9-4d54-b16f-b8dd49e88f90");
        public bool ProvidersInitialized { get; set; } = false;
        public List<ButtonControl> ScreenshotButtonControls { get; set; } = new List<ButtonControl>();
        public List<ScreenshotProviderPlugin> ScreenshotProviders { get; set; } = new List<ScreenshotProviderPlugin>();
        public List<ScreenshotViewerControl> ScreenshotViewerControls { get; set; } = new List<ScreenshotViewerControl>();
        public ScreenshotUtilitiesSettingsViewModel Settings { get; set; }
        public DispatcherTimer Timer { get; private set; }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (!ProvidersInitialized)
            {
                ScreenshotActions.InitializeProviders(this);
            }

            var menuItems = new List<GameMenuItem>();

            if (ScreenshotProviders is null || ScreenshotProviders.Count == 0)
            {
                return menuItems;
            }

            var game = args.Games.FirstOrDefault();

            var menuSection = ResourceProvider.GetString("LOCScreenshotUtilitiesName");

            var providers = new List<ScreenshotProvider>();

            if (CurrentScreenshotsGroups?.ScreenshotCount > 0)
            {
                menuItems.Add(new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuShowScreenshots"),
                    MenuSection = menuSection,
                    Icon = "suShowScreenshotsIcon",
                    Action = a => ScreenshotActions.OpenScreenshotViewer(args.Games.FirstOrDefault(), this)
                });

                menuItems.Add(new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                });

                providers.AddRange(CurrentScreenshotsGroups
                    .Where(g => g.Screenshots?.Count > 0)
                    .Select(g => g.Provider)
                    .Distinct()
                    .OrderBy(p => p.Name));
            }

            menuItems.AddRange(GetDownloadMenuItems(game, providers));

            menuItems.AddRange(GetRefreshMenuItems(game));

            menuItems.AddRange(GetRefreshThumbnailsMenuItems(game));

            menuItems.AddRange(GetResetMenuItems(game, providers));

            menuItems.AddRange(GetSearchMenuItems(game));

            menuItems.AddRange(GetIgnoreMenuItems(game));

            return menuItems;
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
            try
            {
                if (Settings.Settings.Debug)
                {
                    Log.Debug("OnGameSelected triggered!");
                }

                base.OnGameSelected(args);

                if (args?.NewValue?.Count == 1)
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
            catch (Exception ex)
            {
                Log.Error(ex, "ScreenshotUtilities: OnGameSelected error!");
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

        private async Task DownloadScreenshotsAsync(Game game, Guid providerId = default)
        {
            if (await ScreenshotActions.DownloadScreenshotsAsync(game, this, providerId))
            {
                RefreshControls();
            }
        }

        private void Games_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> e)
        {
            foreach (var game in e.RemovedItems)
            {
                ScreenshotHelper.RemoveScreenshots(game);
            }
        }

        private IEnumerable<GameMenuItem> GetDownloadMenuItems(Game game, List<ScreenshotProvider> providers)
        {
            if (providers?.Count == 0)
            {
                yield break;
            }

            var menuCaption = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuDownloadFrom");
            var menuSection = _defaultGameMenuSection;
            var icon = "suDownloadIcon";
            var captionPrefix = $"{menuCaption} ";

            if (providers.Count > 1)
            {
                menuSection = $"{_defaultGameMenuSection}|{menuCaption}...";
                icon = null;
                captionPrefix = string.Empty;

                yield return new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuDownloadScreenshots"),
                    MenuSection = menuSection,
                    Icon = "suDownloadIcon",
                    Action = a => DownloadScreenshotsAsync(game)
                };

                yield return new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                };
            }

            foreach (var provider in providers)
            {
                yield return new GameMenuItem
                {
                    Description = $"{captionPrefix}{provider.Name}",
                    MenuSection = menuSection,
                    Icon = icon,
                    Action = a => DownloadScreenshotsAsync(game, provider.Id)
                };
            }
        }

        private IEnumerable<GameMenuItem> GetIgnoreMenuItems(Game game)
        {
            var providers = ScreenshotProviders
                .OrderBy(p => p.Name)
                .ToList();

            if (providers?.Count == 0)
            {
                yield break;
            }

            var menuCaption = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuIgnoreFrom");
            var menuSection = _defaultGameMenuSection;
            var icon = "suIgnoreIcon";
            var captionPrefix = $"{menuCaption} ";

            if (providers.Count > 1)
            {
                menuSection = $"{_defaultGameMenuSection}|{menuCaption}...";
                icon = null;
                captionPrefix = string.Empty;

                yield return new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuIgnoreGame"),
                    MenuSection = menuSection,
                    Icon = "suIgnoreIcon",
                    Action = a => ScreenshotActions.SetGameToIgnore(game, this)
                };

                yield return new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                };
            }

            foreach (var provider in providers)
            {
                yield return new GameMenuItem
                {
                    Description = $"{captionPrefix}{provider.Name}",
                    MenuSection = menuSection,
                    Icon = icon,
                    Action = a => ScreenshotActions.SetGameToIgnore(game, this, provider.Id)
                };
            }
        }

        private IEnumerable<GameMenuItem> GetRefreshMenuItems(Game game)
        {
            var providers = ScreenshotProviders
                .Where(p => p.SupportsAutomaticScreenshots)
                .OrderBy(p => p.Name)
                .ToList();

            if (providers?.Count == 0)
            {
                yield break;
            }

            var menuCaption = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuRefreshFrom");
            var menuSection = _defaultGameMenuSection;
            var icon = "suFetchIcon";
            var captionPrefix = $"{menuCaption} ";

            if (providers.Count > 1)
            {
                menuSection = $"{_defaultGameMenuSection}|{menuCaption}...";
                icon = null;
                captionPrefix = string.Empty;

                yield return new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuRefreshScreenshots"),
                    MenuSection = menuSection,
                    Icon = "suFetchIcon",
                    Action = a => GetScreenshotsAsync(game)
                };

                yield return new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                };
            }

            foreach (var provider in providers)
            {
                yield return new GameMenuItem
                {
                    Description = $"{captionPrefix}{provider.Name}",
                    MenuSection = menuSection,
                    Icon = icon,
                    Action = a => GetScreenshotsAsync(game, provider.Id)
                };
            }
        }

        private IEnumerable<GameMenuItem> GetRefreshThumbnailsMenuItems(Game game)
        {
            if (CurrentScreenshotsGroups?.ScreenshotCount == 0)
            {
                yield break;
            }

            var providers = CurrentScreenshotsGroups
                                .Where(g => g.Screenshots?.Count(s => s.IsDownloaded) > 0)
                                .Select(g => g.Provider)
                                .Distinct()
                                .OrderBy(p => p.Name)
                                .ToList();

            if (providers?.Count == 0)
            {
                yield break;
            }

            var menuRefresh = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuRefreshThumbnailsFrom");
            var refreshMenuSection = _defaultGameMenuSection;
            var refreshIcon = "suRefreshIcon";
            var refreshPrefix = $"{menuRefresh} ";

            if (providers.Count > 1)
            {
                refreshMenuSection = $"{_defaultGameMenuSection}|{menuRefresh}...";
                refreshIcon = null;
                refreshPrefix = string.Empty;

                yield return new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuRefreshThumbnails"),
                    MenuSection = refreshMenuSection,
                    Icon = "suRefreshIcon",
                    Action = a => RefreshThumbnailsAsync(game)
                };

                yield return new GameMenuItem
                {
                    Description = "-",
                    MenuSection = refreshMenuSection
                };
            }

            foreach (var provider in providers)
            {
                yield return new GameMenuItem
                {
                    Description = $"{refreshPrefix}{provider.Name}",
                    MenuSection = refreshMenuSection,
                    Icon = refreshIcon,
                    Action = a => RefreshThumbnailsAsync(game, provider.Id)
                };
            }
        }

        private IEnumerable<GameMenuItem> GetResetMenuItems(Game game, List<ScreenshotProvider> providers)
        {
            if (providers?.Count == 0)
            {
                yield break;
            }

            var menuCaption = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuResetFrom");
            var menuSection = _defaultGameMenuSection;
            var icon = "suRefreshIcon";
            var captionPrefix = $"{menuCaption} ";

            if (providers.Count > 1)
            {
                menuSection = $"{_defaultGameMenuSection}|{menuCaption}...";
                icon = null;
                captionPrefix = string.Empty;

                yield return new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuResetScreenshots"),
                    MenuSection = menuSection,
                    Icon = "suRefreshIcon",
                    Action = a => ResetScreenshotsAsync(game)
                };

                yield return new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                };
            }

            foreach (var provider in providers)
            {
                yield return new GameMenuItem
                {
                    Description = $"{captionPrefix}{provider.Name}",
                    MenuSection = menuSection,
                    Icon = icon,
                    Action = a => ResetScreenshotsAsync(game, provider.Id)
                };
            }
        }

        private async Task GetScreenshotsAsync(Game game, Guid providerId = default)
        {
            if (await ScreenshotActions.GetScreenshotsAsync(game, this, true, providerId))
            {
                RefreshControls();
            }
        }

        private IEnumerable<GameMenuItem> GetSearchMenuItems(Game game)
        {
            var providers = ScreenshotProviders
                .Where(p => p.SupportsScreenshotSearch)
                .OrderBy(p => p.Name)
                .ToList();

            if (providers?.Count == 0)
            {
                yield break;
            }

            var menuCaption = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuSearchFrom");
            var menuSection = _defaultGameMenuSection;
            var icon = "suSearchIcon";
            var captionPrefix = $"{menuCaption} ";

            if (providers.Count > 1)
            {
                menuSection = $"{_defaultGameMenuSection}|{menuCaption}...";
                icon = null;
                captionPrefix = string.Empty;

                yield return new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCScreenshotUtilitiesMenuSearchScreenshots"),
                    MenuSection = menuSection,
                    Icon = "suSearchIcon",
                    Action = a => SearchScreenshotsAsync(game)
                };

                yield return new GameMenuItem
                {
                    Description = "-",
                    MenuSection = menuSection
                };
            }

            foreach (var provider in providers)
            {
                yield return new GameMenuItem
                {
                    Description = $"{captionPrefix}{provider.Name}",
                    MenuSection = menuSection,
                    Icon = icon,
                    Action = a => SearchScreenshotsAsync(game, provider.Id)
                };
            }
        }

        private async Task RefreshThumbnailsAsync(Game game, Guid providerId = default)
        {
            if (await ScreenshotActions.RefreshThumbnailsAsync(game, this, providerId))
            {
                RefreshControls();
            }
        }

        private async Task ResetScreenshotsAsync(Game game, Guid providerId = default)
        {
            if (API.Instance.Dialogs.ShowMessage(
                        ResourceProvider.GetString("LOCScreenshotUtilitiesDialogResetScreenshots"), string.Empty,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            if (await ScreenshotActions.ResetScreenshots(game, this, providerId))
            {
                RefreshControls();
            }
        }

        private async Task SearchScreenshotsAsync(Game game, Guid providerId = default)
        {
            if (await ScreenshotActions.SearchScreenshotsAsync(game, this, providerId))
            {
                RefreshControls();
            }
        }
    }
}