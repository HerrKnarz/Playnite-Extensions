using Newtonsoft.Json;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the itch.io api search.
namespace LinkUtilities.Models.Itch
{
    public class SearchedGame
    {
        [JsonProperty("cover_url")]
        public string CoverUrl;

        [JsonProperty("p_windows")]
        public bool PWindows;

        [JsonProperty("p_linux")]
        public bool PLinux;

        [JsonProperty("p_osx")]
        public bool POsx;

        [JsonProperty("p_android")]
        public bool PAndroid;

        [JsonProperty("published_at")]
        public string PublishedAt;

        [JsonProperty("created_at")]
        public string CreatedAt;

        [JsonProperty("can_be_bought")]
        public bool CanBeBought;

        [JsonProperty("in_press_system")]
        public bool InPressSystem;

        [JsonProperty("short_text")]
        public string ShortText;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("classification")]
        public string Classification;

        [JsonProperty("min_price")]
        public int MinPrice;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("has_demo")]
        public bool HasDemo;

        [JsonProperty("still_cover_url")]
        public string StillCoverUrl;
    }

    public class ItchSearchResult
    {
        [JsonProperty("per_page")]
        public int PerPage;

        [JsonProperty("page")]
        public int Page;

        [JsonProperty("games")]
        public List<SearchedGame> Games;
    }
}