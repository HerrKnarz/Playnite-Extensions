using KNARZhelper.Enum;

namespace MetadataUtilities.Models
{
    public class WhiteListItem : MetadataObject
    {
        public WhiteListItem(FieldType type, string name = default) : base(type, name) { }

        public bool HideInDetails { get; set; }

        public bool HideInEditor { get; set; }
    }
}
