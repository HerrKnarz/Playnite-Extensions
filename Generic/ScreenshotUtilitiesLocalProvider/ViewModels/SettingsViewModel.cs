using Playnite.SDK;
using Playnite.SDK.Data;
using ScreenshotUtilitiesLocalProvider.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScreenshotUtilitiesLocalProvider.ViewModels
{
    public class SettingsViewModel : ObservableObject, ISettings
    {
        private readonly ScreenshotUtilitiesLocalProvider _plugin;
        private string _exampleName = "Baldur's Gate 3";
        private string _exampleResult = "";
        private FolderConfig _selectedItem;
        private SettingsModel _settings;

        public SettingsViewModel(ScreenshotUtilitiesLocalProvider plugin)
        {
            _plugin = plugin;

            var savedSettings = plugin.LoadPluginSettings<SettingsModel>();

            Settings = savedSettings ?? new SettingsModel();

            if (Settings.FolderConfigs is null)
            {
                Settings.FolderConfigs = new ObservableCollection<FolderConfig>();
            }
            else
            {
                SortFolderConfigs();
            }
        }

        public RelayCommand AddFolderConfigCommand => new RelayCommand(() => Settings.FolderConfigs.Add(new FolderConfig()));

        public string ExampleName
        {
            get => _exampleName;
            set
            {
                _exampleName = value;
                ExampleResult = _selectedItem?.FormatGameName(_exampleName) ?? string.Empty;
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

        public RelayCommand<IList<object>> RemoveFolderConfigCommand => new RelayCommand<IList<object>>(items =>
                        {
                            foreach (var item in items.ToList().Cast<FolderConfig>())
                            {
                                Settings.FolderConfigs.Remove(item);
                            }
                        }, items => items?.Any() ?? false);

        public FolderConfig SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                ExampleResult = _selectedItem?.FormatGameName(_exampleName) ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public SettingsModel Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SortFolderConfigsCommand => new RelayCommand(SortFolderConfigs);

        public RelayCommand TestFolderConfigCommand => new RelayCommand(() =>
            ExampleResult = _selectedItem?.FormatGameName(_exampleName) ?? string.Empty);

        private SettingsModel EditingClone { get; set; }

        public void BeginEdit() =>
            EditingClone = Serialization.GetClone(Settings);

        public void CancelEdit() =>
                        Settings.FolderConfigs = EditingClone.FolderConfigs;

        public void EndEdit()
        {
            _plugin.SavePluginSettings(Settings);

            Settings.FolderConfigs.ForEach(c => c.StringExpander = _plugin.StringExpander);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }

        private void SortFolderConfigs()
        {
            Settings.FolderConfigs = new ObservableCollection<FolderConfig>(Settings.FolderConfigs
                .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList());
        }
    }
}
