using Newtonsoft.Json;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the itch.io api search.
namespace LinkUtilities.Models.ApiResults
{
    public class SearchedGame
    {
        [JsonProperty("can_be_bought")]
        public bool CanBeBought;

        [JsonProperty("classification")]
        public string Classification;

        [JsonProperty("cover_url")]
        public string CoverUrl;

        [JsonProperty("created_at")]
        public string CreatedAt;

        [JsonProperty("has_demo")]
        public bool HasDemo;

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("in_press_system")]
        public bool InPressSystem;

        [JsonProperty("min_price")]
        public int MinPrice;

        [JsonProperty("p_android")]
        public bool PAndroid;

        [JsonProperty("p_linux")]
        public bool PLinux;

        [JsonProperty("p_osx")]
        public bool POsx;

        [JsonProperty("published_at")]
        public string PublishedAt;

        [JsonProperty("p_windows")]
        public bool PWindows;

        [JsonProperty("short_text")]
        public string ShortText;

        [JsonProperty("still_cover_url")]
        public string StillCoverUrl;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("url")]
        public string Url;
    }

    public class ItchSearchResult
    {
        [JsonProperty("games")]
        public List<SearchedGame> Games;

        [JsonProperty("page")]
        public int Page;

        [JsonProperty("per_page")]
        public int PerPage;
    }
}