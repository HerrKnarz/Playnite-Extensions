using KNARZhelper;
using KNARZhelper.GamesCommon;
using Playnite.SDK;
using Playnite.SDK.Data;
using ScreenshotUtilitiesLocalProvider.Models;
using ScreenshotUtilitiesLocalProvider.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScreenshotUtilitiesLocalProvider.ViewModels
{
    public class SettingsViewModel : ObservableObject, ISettings
    {
        private readonly ScreenshotUtilitiesLocalProvider _plugin;
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

        public RelayCommand AddFolderConfigCommand => new RelayCommand(() =>
        {
            var configToEdit = new FolderConfig();

            if (!OpenEditDialog(ref configToEdit))
            {
                return;
            }

            Settings.FolderConfigs.Add(configToEdit);
        });

        public RelayCommand<object> EditFolderConfigCommand => new RelayCommand<object>(item =>
        {
            var selectedConfig = item as FolderConfig;

            var configToEdit = selectedConfig.DeepClone();

            if (!OpenEditDialog(ref configToEdit))
            {
                return;
            }

            selectedConfig.Active = configToEdit.Active;
            selectedConfig.FileMask = configToEdit.FileMask;
            selectedConfig.InvalidCharReplacement = configToEdit.InvalidCharReplacement;
            selectedConfig.Name = configToEdit.Name;
            selectedConfig.Path = configToEdit.Path;
            selectedConfig.RemoveDiacritics = configToEdit.RemoveDiacritics;
            selectedConfig.RemoveEditionSuffix = configToEdit.RemoveEditionSuffix;
            selectedConfig.RemoveHyphens = configToEdit.RemoveHyphens;
            selectedConfig.RemoveSpecialChars = configToEdit.RemoveSpecialChars;
            selectedConfig.RemoveWhitespaces = configToEdit.RemoveWhitespaces;
            selectedConfig.UnderscoresToWhitespaces = configToEdit.UnderscoresToWhitespaces;
            selectedConfig.WhitespacesToHyphens = configToEdit.WhitespacesToHyphens;
            selectedConfig.WhitespacesToUnderscores = configToEdit.WhitespacesToUnderscores;
        });

        public RelayCommand<object> RemoveFolderConfigCommand => new RelayCommand<object>(item => Settings.FolderConfigs.Remove(item as FolderConfig));

        public FolderConfig SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
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

        public bool OpenEditDialog(ref FolderConfig configToEdit)
        {
            try
            {
                configToEdit.StringExpander = _plugin.StringExpander;
                configToEdit.TestGame = new GameEx(API.Instance.MainView.SelectedGames.FirstOrDefault());

                var window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCScreenshotUtilitiesLocalProviderSettingsButtonEdit"), 1200, 800);
                window.Content = new EditFolderConfigView();
                window.DataContext = new EditFolderConfigViewModel(configToEdit);

                return window.ShowDialog() ?? false;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing edit folder config dialog", true);

                return false;
            }
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
