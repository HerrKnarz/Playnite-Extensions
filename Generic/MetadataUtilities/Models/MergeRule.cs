using System.Collections.ObjectModel;

namespace MetadataUtilities.Models
{
    public class MergeRule : MetadataListObject
    {
        private ObservableCollection<MetadataListObject> _sourceObjects;

        public ObservableCollection<MetadataListObject> SourceObjects
        {
            get => _sourceObjects;
            set
            {
                _sourceObjects = value;
                OnPropertyChanged();
            }
        }
    }
}