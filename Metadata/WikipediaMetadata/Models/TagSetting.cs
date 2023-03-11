using Playnite.SDK.Data;

namespace WikipediaMetadata.Models
{
    public class TagSetting
    {
        [SerializationPropertyName("name")]
        public string Name { get; set; }
        [SerializationPropertyName("isChecked")]
        public bool IsChecked { get; set; }
        [SerializationPropertyName("prefix")]
        public string Prefix { get; set; }
    }
}
