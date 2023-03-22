using Newtonsoft.Json;
using System.Collections.Generic;

namespace LinkUtilities.Models.GiantBomb
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Platform
    {
        [JsonProperty("api_detail_url")]
        public string ApiDetailUrl;
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("site_detail_url")]
        public string SiteDetailUrl;

        [JsonProperty("abbreviation")]
        public string Abbreviation;
    }

    public class Result
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("original_release_date")]
        public string OriginalReleaseDate;

        [JsonProperty("platforms")]
        public List<Platform> Platforms;

        [JsonProperty("site_detail_url")]
        public string SiteDetailUrl;

        [JsonProperty("resource_type")]
        public string ResourceType;
    }

    public class GiantBombSearchResult
    {
        [JsonProperty("error")]
        public string Error;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;

        [JsonProperty("number_of_page_results")]
        public int NumberOfPageResults;

        [JsonProperty("number_of_total_results")]
        public int NumberOfTotalResults;

        [JsonProperty("status_code")]
        public int StatusCode;

        [JsonProperty("results")]
        public List<Result> Results;

        [JsonProperty("version")]
        public string Version;
    }
}


