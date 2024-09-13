using KNARZhelper.Enum;
using Playnite.SDK.Data;
using System.Collections.Generic;
using KNARZhelper.DatabaseObjectTypes;

namespace MetadataUtilities.Models
{
    public class FilterType : ObservableObject
    {
        private int _count;
        private FieldType _fieldType;
        private bool _selected = true;

        public FilterType()
        {
            UpdateCount();
        }

        [DontSerialize]
        public int Count
        {
            get => _count;
            set => SetValue(ref _count, value);
        }

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

        public void UpdateCount() => Count = Type.GetTypeManager() is IObjectType type ? type.Count : 0;
    }
}