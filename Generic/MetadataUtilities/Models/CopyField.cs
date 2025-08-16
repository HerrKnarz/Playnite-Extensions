using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace MetadataUtilities.Models
{
    public class CopyField : ObservableObject
    {
        private FieldType _fieldType;
        private bool _copyData = true;
        private bool _replaceData = false;
        private bool _onlyIfEmpty = false;
        private List<Guid> _items = new List<Guid>();

        public CopyField(FieldType fieldType)
        {
            _fieldType = fieldType;

            if (fieldType.GetTypeManager() is IValueType valueType)
            {
                _copyData = valueType.IsDefaultToCopy;
            }

            if (_copyData && !(fieldType.GetTypeManager() is BaseListType))
            {
                _replaceData = true;
            }
        }

        public FieldType FieldType
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

        [DontSerialize]

        public string Label
        {
            get
            {
                var typeManager = FieldType.GetTypeManager();

                return typeManager is BaseListType listType ? listType.LabelPlural : typeManager.LabelSingular;
            }
        }

        public bool CopyToGame(Game sourceGame, Game targetGame, bool replaceValue = false, bool onlyIfEmpty = false) =>
            CopyData
            && FieldType.GetTypeManager() is IValueType valueType
            && valueType.CopyValueToGame(sourceGame, targetGame, replaceValue, onlyIfEmpty);
    }
}
