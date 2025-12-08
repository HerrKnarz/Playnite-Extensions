using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScreenshotUtilities.ViewModels
{
    internal class ButtonControlViewModel : ObservableObject
    {
        private static ScreenshotUtilities _plugin;
        private Game _game;

        public ButtonControlViewModel(ScreenshotUtilities plugin)
        {
            _plugin = plugin;
        }

        public Game Game
        {
            get => _game;
            set
            {
                SetValue(ref _game, value);
                Refresh();
            }
        }

        public RelayCommand<object> OpenViewerCommand => new RelayCommand<object>(a =>
                {
                    if (Game == null)
                    {
                        return;
                    }

                    ScreenshotActions.OpenScreenshotViewer(Game, _plugin);
                });

        public void Refresh() => _plugin.Settings.Settings.IsButtonControlVisible = ButtonIsVisible();

        private bool ButtonIsVisible()
        {
            if (_plugin == null || !_plugin.Settings.Settings.DisplayButtonControl || _game == null || (_plugin.Settings.Settings.CurrentScreenshotGroups?.ScreenshotCount ?? 0) == 0)
            {
                return false;
            }

            var path = Path.Combine(_plugin.GetPluginUserDataPath(), _game.Id.ToString());

            return Directory.Exists(path) && Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories).Any();
        }
    }
}