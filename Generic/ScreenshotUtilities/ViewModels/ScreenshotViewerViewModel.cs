using KNARZhelper;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using ScreenshotUtilities.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenshotUtilities.ViewModels
{
    internal class ScreenshotViewerViewModel : ObservableObject
    {
        private Guid _gameId = Guid.Empty;
        private static ScreenshotUtilities _plugin;
        private ScreenshotGroup _selectedGroup;
        private readonly bool _standaloneMode = false;

        public ScreenshotViewerViewModel(ScreenshotUtilities plugin, Game game = null)
        {
            _plugin = plugin;

            if (game != null)
            {
                _standaloneMode = true;
                GameId = game.Id;
            }
        }

        public static Window GetWindow(ScreenshotUtilities plugin, Game game)
        {
            try
            {
                var screenshotViewerView = new ScreenshotViewerControl(plugin, game)
                {
                    Padding = new Thickness(10)
                };

                var window = WindowHelper.CreateSizedWindow(
                    ResourceProvider.GetString("LOC_ScreenshotUtilities_ControlLabel"),
                    plugin.Settings.Settings.ViewerWindowWidth,
                    plugin.Settings.Settings.ViewerWindowHeight);

                window.Content = screenshotViewerView;
                window.Closing += OnWindowClosing;

                return window;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing screenshot viewer", true);

                return null;
            }
        }

        public void LoadScreenshots()
        {
            ResetViewModel();

            if (!_plugin.Settings.Settings.IsViewerControlVisible && !_standaloneMode)
            {
                return;
            }

            if (ScreenshotGroups == null || (ScreenshotGroups.Count == 0) || !ScreenshotGroups[0].BasePath.Contains(_gameId.ToString()))
            {
                return;
            }

            OnPropertyChanged("ScreenshotGroups");

            SelectedGroup = ScreenshotGroups[0];
        }

        private static void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is Window window)
            {
                _plugin.Settings.Settings.ViewerWindowWidth = (int)window.Width;
                _plugin.Settings.Settings.ViewerWindowHeight = (int)window.Height;
                _plugin.SavePluginSettings(_plugin.Settings.Settings);
            }
        }

        public void ResetViewModel() => SelectedGroup = null;

        public async Task SetAs(MetadataField type)
        {
            if (SelectedGroup?.SelectedScreenshot is null || _gameId == default)
            {
                return;
            }

            if ((SelectedGroup.SelectedScreenshot.DownloadedPath is null)
                && (string.IsNullOrEmpty(SelectedGroup.BasePath)
                    || !await SelectedGroup.SelectedScreenshot.DownloadAsync(SelectedGroup.BasePath)))
            {
                return;
            }

            var game = API.Instance.Database.Games.Get(GameId);

            SelectedGroup.SelectedScreenshot.SetAs(game, type);
        }

        public RelayCommand<object> CopyToClipboardCommand => new RelayCommand<object>(a => SelectedGroup?.SelectedScreenshot?.CopyToClipboard());

        public RelayCommand<object> OpenContainingFolderCommand => new RelayCommand<object>(a =>
        {
            if (string.IsNullOrEmpty(SelectedGroup?.SelectedScreenshot?.DownloadedPath))
            {
                SelectedGroup.OpenContainingFolder();

                return;
            }

            SelectedGroup?.SelectedScreenshot?.OpenContainingFolder();
        });

        public RelayCommand<object> OpenInAssociatedApplicationCommand => new RelayCommand<object>(a => SelectedGroup?.SelectedScreenshot?.OpenInAssociatedApplication());

        public RelayCommand<object> OpenInBrowserCommand => new RelayCommand<object>(a => SelectedGroup?.SelectedScreenshot?.OpenInBrowser());

        public RelayCommand<object> OpenInFullScreenCommand => new RelayCommand<object>(a =>
        {
            if (!(SelectedGroup?.Screenshots?.Count > 0) || SelectedGroup.SelectedScreenshot == null)
            {
                return;
            }

            var window = FullScreenViewModel.GetWindow(_plugin, SelectedGroup);

            if (window == null)
            {
                return;
            }

            window?.ShowDialog();
        });

        public RelayCommand<object> SelectPreviousScreenshotCommand => new RelayCommand<object>(a => SelectedGroup?.SelectPreviousScreenshot());

        public RelayCommand<object> SelectNextScreenshotCommand => new RelayCommand<object>(a => SelectedGroup?.SelectNextScreenshot());

        public RelayCommand<object> SetAsBackgroundCommand => new RelayCommand<object>(a => SetAs(MetadataField.BackgroundImage));

        public RelayCommand<object> SetAsCoverCommand => new RelayCommand<object>(a => SetAs(MetadataField.CoverImage));

        public RelayCommand<object> SetAsIconCommand => new RelayCommand<object>(a => SetAs(MetadataField.Icon));

        public double AspectRatio => _plugin.Settings.Settings.AspectWidth / (float)_plugin.Settings.Settings.AspectHeight;

        public double ButtonHeight => _plugin.Settings.Settings.ThumbnailHeight + 10;

        public double DockPanelHeight => _plugin.Settings.Settings.ThumbnailHeight + 30;

        public Guid GameId
        {
            get => _gameId;
            set
            {
                SetValue(ref _gameId, value);

                LoadScreenshots();
            }
        }

        public ScreenshotGroups ScreenshotGroups => _plugin.CurrentScreenshotsGroups is null ? new ScreenshotGroups() : _plugin.CurrentScreenshotsGroups;

        public ScreenshotGroup SelectedGroup
        {
            get => _selectedGroup;
            set => SetValue(ref _selectedGroup, value);
        }

        public double ThumbnailHeight => _plugin.Settings.Settings.ThumbnailHeight;
    }
}
