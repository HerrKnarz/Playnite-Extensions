using Newtonsoft.Json;
using System.Collections.Generic;

// Contains all necessary classes to deserialize a search request from IGDB with the following fields:
// name, url, release_dates.y, release_dates.platform.name
namespace LinkUtilities.Models
{
    public class Platform
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;
    }

    public class ReleaseDate
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("platform")]
        public Platform Platform;

        [JsonProperty("y")]
        public int Y;
    }

    public class IGDBSearchResult
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("release_dates")]
        public List<ReleaseDate> ReleaseDates;

        [JsonProperty("url")]
        public string Url;
    }
}
