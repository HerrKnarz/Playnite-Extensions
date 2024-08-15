using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class SettableMetadataObject : MetadataObject
    {
        private string _cleanedUpName;
        private string _editName;
        private string _prefix = string.Empty;
        private bool _showGrouped;

        public SettableMetadataObject(Settings settings) : base(settings)
        {
        }

        [DontSerialize]
        public string CleanedUpName
        {
            get => _cleanedUpName;
            set => SetValue(ref _cleanedUpName, value);
        }

        [DontSerialize]
        public string EditName
        {
            get => _editName;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (_editName != value && UpdateItem(Name, Prefix + value))
                {
                    SetValue(ref _editName, value);
                    _name = Prefix + value;
                    CleanedUpName = EditName.RemoveDiacritics().RemoveSpecialChars().ToLower().Replace("-", "").Replace(" ", "");
                }

                OnPropertyChanged();
            }
        }

        public new string Name
        {
            get => _name;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                SetValue(ref _name, value);

                _prefix = GetPrefix();

                if (value.Equals(_prefix))
                {
                    _prefix = string.Empty;
                    _editName = value;
                }
                else
                {
                    _editName = _prefix == string.Empty ? value : value.RemoveFirst(_prefix);
                }

                CleanedUpName = EditName.RemoveDiacritics().RemoveSpecialChars().ToLower().Replace("-", "").Replace(" ", "");
            }
        }

        [DontSerialize]
        public string Prefix
        {
            get => _prefix;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                if (_prefix == value || !UpdateItem(Name, value + EditName))
                {
                    return;
                }

                SetValue(ref _prefix, value);
                _name = value + EditName;
            }
        }

        [DontSerialize]
        public bool ShowGrouped
        {
            get => _showGrouped;
            set => SetValue(ref _showGrouped, value);
        }

        public new SettableFieldType Type
        {
            get => (SettableFieldType)_type;
            set => SetValue(ref _type, (FieldType)value);
        }

        public void CheckGroup(List<SettableMetadataObject> metadataList)
            => ShowGrouped = metadataList.Any(x => x.CleanedUpName == CleanedUpName && !x.Equals(this));

        public string GetPrefix()
        {
            if (_settings?.Prefixes == null)
            {
                return string.Empty;
            }

            foreach (string prefix in _settings.Prefixes)
            {
                if (Name?.StartsWith(prefix) ?? false)
                {
                    return prefix;
                }
            }

            return string.Empty;
        }

        public bool UpdateItem(string oldName, string newName)
        {
            // If we don't have an id, the item is new and doesn't need to be updated.
            if (Id == Guid.Empty)
            {
                return true;
            }

            DbInteractionResult res = DatabaseObjectHelper.UpdateName(Type, Id, oldName, newName);

            if (res == DbInteractionResult.IsDuplicate)
            {
                API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAlreadyExists"),
                    Type.GetEnumDisplayName()));
            }

            return res == DbInteractionResult.Updated;
        }
    }
}