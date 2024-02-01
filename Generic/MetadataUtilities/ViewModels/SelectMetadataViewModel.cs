using MetadataUtilities.Models;
using Playnite.SDK;
using System.Windows;

namespace MetadataUtilities
{
    public class SelectMetadataViewModel : ViewModelBase
    {
        private MetadataListObjects _metadataListObjects;
        private MetadataUtilities _plugin;

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set
            {
                _plugin = value;
                OnPropertyChanged("Plugin");
            }
        }

        public MetadataListObjects MetadataListObjects
        {
            get => _metadataListObjects;
            set
            {
                _metadataListObjects = value;
                OnPropertyChanged("MetadataListObjects");
            }
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public RelayCommand<Window> CancelCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = false;
            win.Close();
        }, win => win != null);
    }
}