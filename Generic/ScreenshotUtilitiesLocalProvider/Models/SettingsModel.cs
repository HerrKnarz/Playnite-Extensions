using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScreenshotUtilitiesLocalProvider.Models
{
    public class SettingsModel : ObservableObject
    {
        private ObservableCollection<FolderConfig> folderConfigs = new ObservableCollection<FolderConfig>();

        public ObservableCollection<FolderConfig> FolderConfigs
        {
            get => folderConfigs;
            set => SetValue(ref folderConfigs, value);
        }
    }
}
