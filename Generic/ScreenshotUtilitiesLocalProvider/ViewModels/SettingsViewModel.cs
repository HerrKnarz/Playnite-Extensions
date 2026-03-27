using KNARZhelper;
using KNARZhelper.GamesCommon;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
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
        private FolderConfig _selectedFolderConfig;
        private GameProfile _selectedGameProfile;
        private SettingsModel _settings;

        public SettingsViewModel(ScreenshotUtilitiesLocalProvider plugin)
        {
            _plugin = plugin;

            var savedSettings = plugin.LoadPluginSettings<SettingsModel>();

            Settings = savedSettings ?? new SettingsModel();

            if (Settings.GameProfiles is null)
            {
                Settings.GameProfiles = new ObservableCollection<GameProfile>
                {
                    new GameProfile(default)
                };
            }
            else
            {
                SortGameProfiles();
                SelectedGameProfile = Settings.GameProfiles.FirstOrDefault();
            }
        }

        public RelayCommand<object> AddFolderConfigCommand => new RelayCommand<object>(item =>
        {
            if (item == null)
            {
                return;
            }

            SelectedGameProfile = item as GameProfile;

            var configToEdit = new FolderConfig();

            if (!OpenEditDialog(ref configToEdit, SelectedGameProfile.Game))
            {
                return;
            }

            SelectedGameProfile.FolderConfigs.Add(configToEdit);

            SelectedGameProfile.FolderConfigs.Sort(x => x.Name);

            SelectedFolderConfig = configToEdit;
        });

        public RelayCommand AddGameProfileCommand => new RelayCommand(() =>
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

            var ProfileToAdd = Settings.GameProfiles.FirstOrDefault(p => p.GameId == tempGame.Game.Id);

            if (ProfileToAdd == null)
            {
                ProfileToAdd = new GameProfile
                {
                    GameId = tempGame.Game.Id
                };

                Settings.GameProfiles.Add(ProfileToAdd);

                SortGameProfiles(false);
            }

            SelectedGameProfile = ProfileToAdd;
        });

        public RelayCommand<object> EditFolderConfigCommand => new RelayCommand<object>(item =>
        {
            var selectedConfig = item as FolderConfig;

            var configToEdit = selectedConfig.DeepClone();

            if (!OpenEditDialog(ref configToEdit, SelectedGameProfile.Game))
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

        public RelayCommand<object> RemoveFolderConfigCommand => new RelayCommand<object>(item => SelectedGameProfile.FolderConfigs.Remove(item as FolderConfig));

        public RelayCommand<object> RemoveGameProfileCommand => new RelayCommand<object>(item => Settings.GameProfiles.Remove(item as GameProfile));

        public FolderConfig SelectedFolderConfig
        {
            get => _selectedFolderConfig;
            set
            {
                _selectedFolderConfig = value;
                OnPropertyChanged();
            }
        }

        public GameProfile SelectedGameProfile
        {
            get => _selectedGameProfile;
            set
            {
                _selectedGameProfile = value;
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

        public bool OpenEditDialog(ref FolderConfig configToEdit, Game game = null)
        {
            try
            {
                configToEdit.StringExpander = _plugin.StringExpander;
                configToEdit.TestGame = new GameEx(game);

                var window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCScreenshotUtilitiesLocalProviderSettingsButtonEdit"), 1200, 800);
                window.Content = new EditFolderConfigView();
                window.DataContext = new EditFolderConfigViewModel(configToEdit, SelectedGameProfile.GameId != default);

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

        private void SortGameProfiles(bool sortConfigs = true)
        {
            Settings.GameProfiles.Sort(p => $"{(p.GameId == default ? 1 : 2)}#{p.Game?.Name ?? string.Empty}");

            if (sortConfigs)
            {
                Settings.GameProfiles.ForEach(p => p.FolderConfigs.Sort(x => x.Name));
            }
        }
    }
}
