using KNARZhelper;
using Playnite.SDK.Models;

namespace AdvancedMetadataTools.Models
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
                if (_editName == null || _editName == value || DatabaseObjectHelper.UpdateName(Type, Id, _editName, value))
                {
                    _editName = value;
                }

                OnPropertyChanged();
            }
        }

        public string TypeLabel => Type.ToString();
    }
}