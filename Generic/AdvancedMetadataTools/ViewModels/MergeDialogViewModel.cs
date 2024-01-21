using AdvancedMetadataTools.Models;
using Playnite.SDK;
using System.Linq;
using System.Windows;

namespace AdvancedMetadataTools
{
    public class MergeDialogViewModel : ViewModelBase
    {
        private MetadataListObject _mergeDestination;
        private MetadataListObjects _metadataListObjects;
        private AdvancedMetadataTools _plugin;

        public AdvancedMetadataTools Plugin
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
            API.Instance.Dialogs.ShowMessage(MergeDestination.EditName);
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