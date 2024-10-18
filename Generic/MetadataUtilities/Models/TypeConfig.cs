namespace MetadataUtilities.Models
{
    public class TypeConfig : FilterType
    {
        private bool _hiddenAsUnused;
        private bool _removeUnusedItems;

        public bool HiddenAsUnused
        {
            get => _hiddenAsUnused;
            set => SetValue(ref _hiddenAsUnused, value);
        }

        public bool RemoveUnusedItems
        {
            get => _removeUnusedItems;
            set => SetValue(ref _removeUnusedItems, value);
        }
    }
}
