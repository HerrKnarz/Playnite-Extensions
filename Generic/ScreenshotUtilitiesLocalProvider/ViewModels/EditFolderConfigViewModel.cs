using KNARZhelper;
using KNARZhelper.GamesCommon;
using Playnite.SDK;
using ScreenshotUtilitiesLocalProvider.Models;
using System.Collections.Generic;
using System.Windows;

namespace ScreenshotUtilitiesLocalProvider.ViewModels
{
    public class EditFolderConfigViewModel : ObservableObject
    {
        // TODO: Make list of placeholders translatable.

        private FolderConfig _folderConfig;
        private StringPlaceholder _selectedItem;

        public EditFolderConfigViewModel(FolderConfig folderConfig)
        {
            FolderConfig = folderConfig;
        }

        public RelayCommand CopyPlaceholderCommand => new RelayCommand(() =>
        {
            if (SelectedPlaceholder == null)
            {
                return;
            }

            Clipboard.SetText(SelectedPlaceholder.Placeholder);
        });

        public FolderConfig FolderConfig
        {
            get => _folderConfig;
            set => SetValue(ref _folderConfig, value);
        }

        public RelayCommand<Window> SaveCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public StringPlaceholder SelectedPlaceholder
        {
            get => _selectedItem;
            set => SetValue(ref _selectedItem, value);
        }

        public RelayCommand SelectGameCommand => new RelayCommand(() =>
        {
            var settings = new GameSearchSettings();

            var tempGame = new GameEx();

            var viewModel = new SearchGameViewModel(settings, null, false, ResourceProvider.GetString("LOCOKLabel"), tempGame);

            var searchGameView = new SearchGameView();

            var window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCSearchLabel"),
                settings.GameSearchWindowWidth, settings.GameSearchWindowHeight);
            window.Content = searchGameView;
            window.DataContext = viewModel;

            if (!window.ShowDialog() ?? true)
            {
                return;
            }

            if (_folderConfig != null)
            {
                _folderConfig.TestGame = tempGame;
            }
        });

        public RelayCommand TestConfigCommand => new RelayCommand(() =>
        {
            _folderConfig?.ResolveFormat();
            _folderConfig?.ResolveConfig();
        });

        private class GameSearchSettings : IGameSearchSettings
        {
            public bool GameGridShowCompletionStatus { get; set; } = true;
            public bool GameGridShowHidden { get; set; } = true;
            public bool GameGridShowPlatform { get; set; } = true;
            public bool GameGridShowReleaseYear { get; set; } = true;
            public int GameSearchWindowHeight { get; set; } = 700;
            public int GameSearchWindowWidth { get; set; } = 700;
        }
    }
}
