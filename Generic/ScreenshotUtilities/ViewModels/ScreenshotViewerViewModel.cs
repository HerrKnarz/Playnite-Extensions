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
        private Screenshot _selectedScreenshot;
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
            SelectedScreenshot = null;
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

            _plugin.Settings.Settings.IsControlVisible = true;

            return;
        }

        public static Window GetWindow(ScreenshotUtilities plugin, Game game)
        {
            try
            {
                var screenshotViewerView = new ScreenshotViewerControl(plugin, game);

                var window = WindowHelper.CreateSizedWindow(
                    "Screenshots",
                    800, 600);

                window.Content = screenshotViewerView;

                return window;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing edit conditional action dialog", true);

                return null;
            }
        }

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
            set
            {
                SetValue(ref _selectedGroup, value);

                SelectedScreenshot = value != null && value.Screenshots.Count > 0 ? value.Screenshots[0] : null;
            }
        }

        public Screenshot SelectedScreenshot
        {
            get => _selectedScreenshot;
            set => SetValue(ref _selectedScreenshot, value);
        }
    }
}
