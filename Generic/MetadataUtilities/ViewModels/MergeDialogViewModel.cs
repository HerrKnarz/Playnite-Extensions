using MetadataUtilities.Models;
using Playnite.SDK;
using System.Linq;
using System.Windows;

namespace MetadataUtilities
{
    public class MergeDialogViewModel : ViewModelBase
    {
        private MetadataListObject _mergeDestination;
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

                MergeDestination = _metadataListObjects.FirstOrDefault();

                OnPropertyChanged("MetadataListObjects");
            }
        }

        public MetadataListObject MergeDestination
        {
            get => _mergeDestination;
            set
            {
                _mergeDestination = value;
                OnPropertyChanged("MergeDestination");
            }
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            MetadataListObjects.MergeItems(MergeDestination.Type, MergeDestination.Id);
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