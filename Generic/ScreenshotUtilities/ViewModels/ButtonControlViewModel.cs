using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScreenshotUtilities.ViewModels
{
    internal class ButtonControlViewModel : ObservableObject
    {
        private Game _game;
        private static ScreenshotUtilities _plugin;

        public ButtonControlViewModel(ScreenshotUtilities plugin)
        {
            _plugin = plugin;
        }

        public RelayCommand<object> OpenViewerCommand => new RelayCommand<object>(a =>
        {
            if (Game == null)
            {
                return;
            }

            ScreenshotActions.OpenScreenshotViewer(Game, _plugin);
        });

        public Game Game
        {
            get => _game;
            set
            {
                SetValue(ref _game, value);
                _plugin.Settings.Settings.IsButtonControlVisible = ButtonIsVisible();
            }
        }

        private bool ButtonIsVisible()
        {
            if (_plugin == null || !_plugin.Settings.Settings.DisplayButtonControl || _game == null)
            {
                return false;
            }

            var path = Path.Combine(_plugin.GetPluginUserDataPath(), _game.Id.ToString());

            return Directory.Exists(path) && Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories).Any();
        }
    }
}
