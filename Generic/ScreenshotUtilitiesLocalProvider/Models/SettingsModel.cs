using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScreenshotUtilitiesLocalProvider.Models
{
    public class SettingsModel : ObservableObject
    {
        private ObservableCollection<FolderConfig> _folderConfigs = new ObservableCollection<FolderConfig>();

        private ObservableCollection<GameProfile> _gameProfiles = new ObservableCollection<GameProfile>();

        public ObservableCollection<FolderConfig> FolderConfigs
        {
            // TODO: Remove once I fully migrated to GameProfiles
            get => _folderConfigs;
            set => SetValue(ref _folderConfigs, value);
        }

        public ObservableCollection<GameProfile> GameProfiles
        {
            get => _gameProfiles;
            set => SetValue(ref _gameProfiles, value);
        }
    }
}
