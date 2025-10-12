using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using ScreenshotUtilities.Controls;
using ScreenshotUtilities.Models;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ScreenshotUtilities.ViewModels
{
    internal class ScreenshotViewerViewModel : ObservableObject
    {
        private Guid _gameId = Guid.Empty;
        private static ScreenshotUtilities _plugin;
        private ScreenshotGroups _screenshotGroups = new ScreenshotGroups();
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

            if (!_plugin.Settings.Settings.DisplayViewerControl && !_standaloneMode)
            {
                return;
            }

            ScreenshotGroups.CreateGroupsFromFiles(_plugin.GetPluginUserDataPath(), _gameId, false);

            if (ScreenshotGroups.Count == 0)
            {
                return;
            }

            SelectedGroup = ScreenshotGroups[0];

            if (_plugin.Settings.Settings.DisplayViewerControl && !_standaloneMode)
            {
                _plugin.Settings.Settings.IsViewerControlVisible = true;
            }

            return;
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

        public void ResetViewModel()
        {
            _plugin.Settings.Settings.IsViewerControlVisible = false;
            ScreenshotGroups.Reset();
            SelectedGroup = null;
        }

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

        public double AspectRatio => _plugin.Settings.Settings.AspectWidth / (float)_plugin.Settings.Settings.AspectHeight;

        public Guid GameId
        {
            get => _gameId;
            set
            {
                SetValue(ref _gameId, value);

                LoadScreenshots();
            }
        }

        public ScreenshotGroups ScreenshotGroups
        {
            get => _screenshotGroups;
            set => SetValue(ref _screenshotGroups, value);
        }

        public ScreenshotGroup SelectedGroup
        {
            get => _selectedGroup;
            set => SetValue(ref _selectedGroup, value);
        }
    }
}
