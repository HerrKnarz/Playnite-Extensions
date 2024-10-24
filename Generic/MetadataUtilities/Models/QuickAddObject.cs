using KNARZhelper.Enum;

namespace MetadataUtilities.Models
{
    public class QuickAddObject : MetadataObject
    {
        public QuickAddObject(FieldType type, string name = default) : base(type, name)
        {
        }

        public bool Add { get; set; } = false;
        public string CustomPath { get; set; } = string.Empty;
        public bool Remove { get; set; } = false;
        public bool Toggle { get; set; } = false;
    }
}