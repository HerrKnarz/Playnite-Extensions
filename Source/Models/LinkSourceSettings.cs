using Newtonsoft.Json;
using System.Windows;

namespace LinkUtilities.Models
{
    public class LinkSourceSetting
    {
        [JsonProperty("linkName")]
        public string LinkName { get; set; }
        [JsonProperty("isAddable")]
        public bool? IsAddable { get; set; }
        [JsonProperty("isSearchable")]
        public bool? IsSearchable { get; set; }
        [JsonIgnore]
        public Visibility IsAddableVisible { get => (IsAddable != null) ? Visibility.Visible : Visibility.Hidden; }
        [JsonIgnore]
        public Visibility IsSearchableVisible { get => (IsSearchable != null) ? Visibility.Visible : Visibility.Hidden; }
    }
}
