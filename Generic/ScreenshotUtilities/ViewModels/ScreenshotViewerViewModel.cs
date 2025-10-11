using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using ScreenshotUtilities.Controls;
using ScreenshotUtilities.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace ScreenshotUtilities.ViewModels
{
    internal class ScreenshotViewerViewModel : ObservableObject
    {
        private Guid _gameId = Guid.Empty;
        private static ScreenshotUtilities _plugin;
        private ObservableCollection<ScreenshotGroup> _screenshotGroups = new ObservableCollection<ScreenshotGroup>();
        private ScreenshotGroup _selectedGroup;

        public ScreenshotViewerViewModel(ScreenshotUtilities plugin, Game game = null)
        {
            _plugin = plugin;
            GameId = game?.Id ?? Guid.Empty;
        }

        public void ResetViewModel()
        {
            _plugin.Settings.Settings.IsControlVisible = false;
            ScreenshotGroups.Clear();
            SelectedGroup = null;
        }

        public void LoadScreenshots()
        {
            ResetViewModel();

            if (_gameId == Guid.Empty)
            {
                ScreenshotGroups.Add(new ScreenshotGroup(ResourceProvider.GetString("LOCScreenshotUtilitiesMessageNoGameSelected")));
                return;
            }

            var path = Path.Combine(_plugin.GetPluginUserDataPath(), _gameId.ToString());

            if (!Directory.Exists(path))
            {
                ScreenshotGroups.Add(new ScreenshotGroup(ResourceProvider.GetString("LOCScreenshotUtilitiesMessageNoScreenshotsFound")));
                return;
            }

            var files = Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories);

            if (!files.Any())
            {
                ScreenshotGroups.Add(new ScreenshotGroup(ResourceProvider.GetString("LOCScreenshotUtilitiesMessageNoScreenshotsFound")));
                return;
            }

            foreach (var file in files)
            {
                try
                {
                    ScreenshotGroups.Add(Serialization.FromJsonFile<ScreenshotGroup>(file));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to load screenshots from file {file}");
                }
            }

            if (ScreenshotGroups.Count == 0)
            {
                return;
            }

            SelectedGroup = ScreenshotGroups[0];

            if (_plugin.Settings.Settings.DisplayViewerControl)
            {
                _plugin.Settings.Settings.IsControlVisible = true;
            }

            return;
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

        private static void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is Window window)
            {
                _plugin.Settings.Settings.ViewerWindowWidth = (int)window.Width;
                _plugin.Settings.Settings.ViewerWindowHeight = (int)window.Height;
                _plugin.SavePluginSettings(_plugin.Settings.Settings);
            }
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

        public Guid GameId
        {
            get => _gameId;
            set
            {
                SetValue(ref _gameId, value);

                LoadScreenshots();
            }
        }

        public ObservableCollection<ScreenshotGroup> ScreenshotGroups
        {
            get => _screenshotGroups;
            set => SetValue(ref _screenshotGroups, value);
        }

        public ScreenshotGroup SelectedGroup
        {
            get => _selectedGroup;
            set => SetValue(ref _selectedGroup, value);
        }

        public double AspectRatio
        {
            get
            {
                double aspectWidth = _plugin.Settings.Settings.AspectWidth;
                double aspectHeight = _plugin.Settings.Settings.AspectHeight;
                return aspectWidth / aspectHeight;
            }
        }
    }
}
