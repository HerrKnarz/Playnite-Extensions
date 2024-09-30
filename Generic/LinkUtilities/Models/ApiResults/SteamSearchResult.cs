using Newtonsoft.Json;

// Contains the class to deserialize the steam search result.
namespace LinkUtilities.Models.ApiResults
{
    public class SteamSearchResult
    {
        [JsonProperty("appid")]
        public string Appid;

        [JsonProperty("icon")]
        public string Icon;

        [JsonProperty("logo")]
        public string Logo;

        [JsonProperty("name")]
        public string Name;
    }
}