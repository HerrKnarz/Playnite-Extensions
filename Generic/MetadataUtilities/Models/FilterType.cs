using KNARZhelper.Enum;
using System.Collections.Generic;

namespace MetadataUtilities.Models
{
    public class FilterType : ObservableObject
    {
        private FieldType _fieldType;
        private bool _selected = true;

        public bool Selected
        {
            get => _selected;
            set => SetValue(ref _selected, value);
        }

        public FieldType Type
        {
            get => _fieldType;
            set => SetValue(ref _fieldType, value);
        }
    }
}