using Playnite.SDK.Models;

namespace AdvancedMetadataTools.Models
{
    public class MetadataListObject : DatabaseObject
    {
        public int GameCount { get; set; }
        public FieldType Type { get; set; }
        public string TypeLabel => Type.ToString();
    }
}