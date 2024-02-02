using MetadataUtilities.Models;
using Playnite.SDK;
using System.Linq;
using System.Windows;

namespace MetadataUtilities
{
    public class AddNewObjectViewModel : ViewModelBase
    {
        //TODO: Try to add localization to combobox!

        private MetadataListObject _newObject = new MetadataListObject();
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

        public MetadataListObject NewObject
        {
            get => _newObject;
            set
            {
                _newObject = value;
                OnPropertyChanged("NewObject");
            }
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = NewObject.Name?.Any() ?? false;
            win.Close();
        }, win => win != null);
    }
}