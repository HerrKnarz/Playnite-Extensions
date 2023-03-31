using Newtonsoft.Json;
using System;

namespace WikipediaMetadata.Models
{
    /// Contains classes needed for the JSON result to fetch a single page.
    public class Latest
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("timestamp")]
        public DateTime Timestamp;
    }

    public class License
    {
        [JsonProperty("url")]
        public string Url;

        [JsonProperty("title")]
        public string Title;
    }

    public class WikipediaPage
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("key")]
        public string Key;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("latest")]
        public Latest Latest;

        [JsonProperty("content_model")]
        public string ContentModel;

        [JsonProperty("license")]
        public License License;

        [JsonProperty("source")]
        public string Source;
    }
}