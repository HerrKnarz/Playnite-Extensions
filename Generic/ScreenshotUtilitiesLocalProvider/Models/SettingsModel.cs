using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScreenshotUtilitiesLocalProvider.Models
{
    public class SettingsModel : ObservableObject
    {
        private ObservableCollection<FolderConfig> _folderConfigs;

        private ObservableCollection<GameProfile> _gameProfiles = new ObservableCollection<GameProfile>();

        /// <summary>
        /// Deprecated - will be removed in a later version, once user had a chance to migrate to
        /// the new GameProfile logic.
        /// </summary>
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
