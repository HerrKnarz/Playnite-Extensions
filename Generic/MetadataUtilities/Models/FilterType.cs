using KNARZhelper.Enum;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.ComponentModel;

namespace MetadataUtilities.Models
{
    public class FilterType : ObservableObject
    {
        private FieldType _fieldType;
        private bool _selected = true;

        [DontSerialize]
        public string Label => Type.GetTypeManager().LabelPlural;

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