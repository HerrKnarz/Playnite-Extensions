using Newtonsoft.Json;
using System.Collections.Generic;

namespace LinkUtilities.Models.ApiResults
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Platform
    {
        [JsonProperty("abbreviation")]
        public string Abbreviation;

        [JsonProperty("api_detail_url")]
        public string ApiDetailUrl;

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("site_detail_url")]
        public string SiteDetailUrl;
    }

    public class Result
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("original_release_date")]
        public string OriginalReleaseDate;

        [JsonProperty("platforms")]
        public List<Platform> Platforms;

        [JsonProperty("resource_type")]
        public string ResourceType;

        [JsonProperty("site_detail_url")]
        public string SiteDetailUrl;
    }

    public class GiantBombSearchResult
    {
        [JsonProperty("error")]
        public string Error;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("number_of_page_results")]
        public int NumberOfPageResults;

        [JsonProperty("number_of_total_results")]
        public int NumberOfTotalResults;

        [JsonProperty("offset")]
        public int Offset;

        [JsonProperty("results")]
        public List<Result> Results;

        [JsonProperty("status_code")]
        public int StatusCode;

        [JsonProperty("version")]
        public string Version;
    }
}