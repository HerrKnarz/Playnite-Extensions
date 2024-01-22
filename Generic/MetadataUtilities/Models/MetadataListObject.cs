using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;

namespace MetadataUtilities.Models
{
    public class MetadataListObject : DatabaseObject
    {
        private string _editName;
        public int GameCount { get; set; }
        public FieldType Type { get; set; }

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
                }

                OnPropertyChanged();
            }
        }

        public string TypeAndName => $"{Type.GetEnumDisplayName("MetadataUtilities")}: {EditName}";
        public string TypeLabel => Type.GetEnumDisplayName("MetadataUtilities");
    }
}