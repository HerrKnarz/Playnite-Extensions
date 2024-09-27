using Playnite.SDK.Data;

namespace WikipediaMetadata.Models
{
    public class TagSetting
    {
        [SerializationPropertyName("isChecked")]
        public bool IsChecked { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("prefix")]
        public string Prefix { get; set; }
    }
}