using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScreenshotUtilitiesLocalProvider.Models
{
    public class SettingsModel : ObservableObject
    {
        private ObservableCollection<GameProfile> _gameProfiles = new ObservableCollection<GameProfile>();

        public ObservableCollection<GameProfile> GameProfiles
        {
            get => _gameProfiles;
            set => SetValue(ref _gameProfiles, value);
        }
    }
}
