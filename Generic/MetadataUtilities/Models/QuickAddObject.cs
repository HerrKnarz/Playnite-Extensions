namespace MetadataUtilities.Models
{
    public class QuickAddObject : SettableMetadataObject
    {
        public QuickAddObject(Settings settings) : base(settings)
        {
        }

        public bool Add { get; set; } = false;
        public string CustomPath { get; set; } = string.Empty;
        public bool Remove { get; set; } = false;
        public bool Toggle { get; set; } = false;
    }
}