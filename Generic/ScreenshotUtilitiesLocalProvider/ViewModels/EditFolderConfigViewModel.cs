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

        private readonly bool _specificGame;
        private FolderConfig _folderConfig;
        private StringPlaceholder _selectedItem;

        public EditFolderConfigViewModel(FolderConfig folderConfig, bool specificGame = true)
        {
            FolderConfig = folderConfig;
            _specificGame = specificGame;
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

        public Visibility HiddenForSpecificGame => _specificGame ? Visibility.Collapsed : Visibility.Visible;

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

            if (_folderConfig != null && tempGame?.Game != null)
            {
                _folderConfig.TestGame = tempGame;
            }
        });

        public RelayCommand TestConfigCommand => new RelayCommand(() =>
        {
            _folderConfig?.ResolveFormat();
            _folderConfig?.ResolveConfig();
        });
    }
}
