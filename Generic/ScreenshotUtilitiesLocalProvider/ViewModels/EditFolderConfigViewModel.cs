using KNARZhelper;
using Playnite.SDK;
using ScreenshotUtilitiesLocalProvider.Models;
using System.Collections.Generic;
using System.Windows;

namespace ScreenshotUtilitiesLocalProvider.ViewModels
{
    public class EditFolderConfigViewModel : ObservableObject
    {
        // TODO: Make list of placeholders translatable.

        private string _exampleName = "Baldur's Gate 3";
        private string _exampleResult = "";
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

        public string ExampleName
        {
            get => _exampleName;
            set
            {
                _exampleName = value;
                ExampleResult = _folderConfig?.FormatGameName(_exampleName) ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string ExampleResult
        {
            get => _exampleResult;
            set
            {
                _exampleResult = value;
                OnPropertyChanged();
            }
        }

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
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand TestFolderConfigCommand => new RelayCommand(() =>
            ExampleResult = _folderConfig?.FormatGameName(_exampleName) ?? string.Empty);
    }
}
