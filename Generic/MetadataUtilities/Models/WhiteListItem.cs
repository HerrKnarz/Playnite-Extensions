using KNARZhelper.Enum;

namespace MetadataUtilities.Models
{
    public class WhiteListItem : MetadataObject
    {
        public WhiteListItem(Settings settings, FieldType type, string name = default) : base(settings, type, name) { }

        public bool HideInDetails { get; set; }

        public bool HideInEditor { get; set; }
    }
}
