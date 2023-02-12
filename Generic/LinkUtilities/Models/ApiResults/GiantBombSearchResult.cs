using Playnite.SDK.Data;
using System.Collections.Generic;

namespace LinkUtilities.Models.GiantBomb
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Platform
    {
        [SerializationPropertyName("api_detail_url")]
        public string ApiDetailUrl;

        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("name")]
        public string Name;

        [SerializationPropertyName("site_detail_url")]
        public string SiteDetailUrl;

        [SerializationPropertyName("abbreviation")]
        public string Abbreviation;
    }

    public class Result
    {
        [SerializationPropertyName("name")]
        public string Name;

        [SerializationPropertyName("original_release_date")]
        public string OriginalReleaseDate;

        [SerializationPropertyName("platforms")]
        public List<Platform> Platforms;

        [SerializationPropertyName("site_detail_url")]
        public string SiteDetailUrl;

        [SerializationPropertyName("resource_type")]
        public string ResourceType;
    }

    public class GiantBombSearchResult
    {
        [SerializationPropertyName("error")]
        public string Error;

        [SerializationPropertyName("limit")]
        public int Limit;

        [SerializationPropertyName("offset")]
        public int Offset;

        [SerializationPropertyName("number_of_page_results")]
        public int NumberOfPageResults;

        [SerializationPropertyName("number_of_total_results")]
        public int NumberOfTotalResults;

        [SerializationPropertyName("status_code")]
        public int StatusCode;

        [SerializationPropertyName("results")]
        public List<Result> Results;

        [SerializationPropertyName("version")]
        public string Version;
    }
}


