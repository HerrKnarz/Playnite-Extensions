using KNARZhelper.DatabaseObjectTypes;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace MetadataUtilities.Models
{
    public class CopyField : ObservableObject
    {
        private IMetadataFieldType _fieldType;
        private bool _copyData = true;
        private bool _replaceData = false;
        private bool _onlyIfEmpty = false;
        private List<Guid> _items = new List<Guid>();

        public CopyField(IMetadataFieldType fieldType)
        {
            _fieldType = fieldType ?? throw new ArgumentNullException(nameof(fieldType));
        }

        public IMetadataFieldType FieldType
        {
            get => _fieldType;
            set => SetValue(ref _fieldType, value);
        }

        public bool CopyData
        {
            get => _copyData;
            set => SetValue(ref _copyData, value);
        }

        public bool ReplaceData
        {
            get => _replaceData;
            set => SetValue(ref _replaceData, value);
        }

        public bool OnlyIfEmpty
        {
            get => _onlyIfEmpty;
            set => SetValue(ref _onlyIfEmpty, value);
        }

        [DontSerialize]
        public List<Guid> Items
        {
            get => _items;
            set => SetValue(ref _items, value ?? new List<Guid>());
        }
    }
}
