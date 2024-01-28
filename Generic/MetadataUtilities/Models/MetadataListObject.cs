using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;

namespace MetadataUtilities.Models
{
    public class MetadataListObject : DatabaseObject
    {
        private string _editName;

        [DontSerialize]
        public new Guid Id { get; set; }

        [DontSerialize]
        public int GameCount { get; set; }

        public FieldType Type { get; set; }

        [DontSerialize]
        public string EditName
        {
            get => _editName;
            set
            {
                if (_editName != null && _editName != value)
                {
                    DbInteractionResult res = DatabaseObjectHelper.UpdateName(Type, Id, _editName, value);

                    switch (res)
                    {
                        case DbInteractionResult.Updated:
                            _editName = value;
                            Name = value;
                            break;
                        case DbInteractionResult.IsDuplicate:
                            API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAlreadyExists"),
                                Type.GetEnumDisplayName("MetadataUtilities")));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    _editName = value;
                    Name = value;
                }

                OnPropertyChanged();
            }
        }

        [DontSerialize]
        public string TypeAndName => $"{Type.GetEnumDisplayName("MetadataUtilities")}: {EditName}";

        [DontSerialize]
        public string TypeLabel => Type.GetEnumDisplayName("MetadataUtilities");
    }
}