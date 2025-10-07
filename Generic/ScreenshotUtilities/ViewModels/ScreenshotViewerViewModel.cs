using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using ScreenshotUtilities.Models;
using ScreenshotUtilities.Views;
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
        private ObservableCollection<Screenshot> _screenshots = new ObservableCollection<Screenshot>();
        private static ScreenshotUtilities _plugin;
        private Screenshot _selectedScreenshot;

        public ScreenshotViewerViewModel(ScreenshotUtilities plugin, Game game)
        {
            _plugin = plugin;

            LoadScreenshots(game);
        }

        public void LoadScreenshots(Game game)
        {
            Screenshots.Clear();

            if (game == null)
            {
                API.Instance.Dialogs.ShowMessage("No game selected");
                return;
            }

            var path = Path.Combine(_plugin.GetPluginUserDataPath(), game.Id.ToString());

            if (!Directory.Exists(path))
            {
                API.Instance.Dialogs.ShowMessage("No screenshots found for this game");
                return;
            }

            var ext = new List<string> { "jpg", "jpeg", "gif", "png", "webp" };
            var files = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(file => ext.Contains(Path.GetExtension(file).TrimStart('.').ToLowerInvariant()));

            if (!files.Any())
            {
                API.Instance.Dialogs.ShowMessage("No screenshots found for this game");
                return;
            }

            Screenshots.AddMissing(files.Select(file => new Screenshot(file)));

            // Test for loading an image from an URL
            //Screenshots.Add(new Screenshot("https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/1091500/ss_0e64170751e1ae20ff8fdb7001a8892fd48260e7.1920x1080.jpg"));

            return;
        }

        public static Window GetWindow(ScreenshotUtilities plugin, Game game)
        {
            try
            {
                var viewModel =
                    new ScreenshotViewerViewModel(plugin, game);

                var screenshotViewerView = new ScreenshotViewerView();

                var window = WindowHelper.CreateSizedWindow(
                    "Screenshots",
                    800, 600);

                window.Content = screenshotViewerView;
                window.DataContext = viewModel;

                return window;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing edit conditional action dialog", true);

                return null;
            }
        }

        public ObservableCollection<Screenshot> Screenshots
        {
            get => _screenshots;
            set => SetValue(ref _screenshots, value);
        }

        public Screenshot SelectedScreenshot
        {
            get => _selectedScreenshot;
            set => SetValue(ref _selectedScreenshot, value);
        }
    }
}
