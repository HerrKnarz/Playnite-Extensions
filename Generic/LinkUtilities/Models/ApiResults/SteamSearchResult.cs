using Newtonsoft.Json;

// Contains the class to deserialize the steam search result.
namespace LinkUtilities.Models.Steam
{
    public class SteamSearchResult
    {
        [JsonProperty("appid")]
        public string Appid;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("icon")]
        public string Icon;

        [JsonProperty("logo")]
        public string Logo;
    }
}