using System.Collections.ObjectModel;

namespace MetadataUtilities.Models
{
    public class MergeRule : MetadataListObject
    {
        public ObservableCollection<MetadataListObject> SourceObjects;
    }
}