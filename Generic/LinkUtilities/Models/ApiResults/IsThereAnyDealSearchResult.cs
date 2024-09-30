using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace LinkUtilities.Models.ApiResults
{
    public class IsThereAnyDealSearchResult
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("mature")]
        public bool Mature;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("type")]
        public string Type;
    }
}