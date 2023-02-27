using Playnite.SDK.Data;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the itch.io api search.
namespace LinkUtilities.Models.Itch
{
    public class SearchedGame
    {
        [SerializationPropertyName("cover_url")]
        public string CoverUrl;

        [SerializationPropertyName("p_windows")]
        public bool PWindows;

        [SerializationPropertyName("p_linux")]
        public bool PLinux;

        [SerializationPropertyName("p_osx")]
        public bool POsx;

        [SerializationPropertyName("p_android")]
        public bool PAndroid;

        [SerializationPropertyName("published_at")]
        public string PublishedAt;

        [SerializationPropertyName("created_at")]
        public string CreatedAt;

        [SerializationPropertyName("can_be_bought")]
        public bool CanBeBought;

        [SerializationPropertyName("in_press_system")]
        public bool InPressSystem;

        [SerializationPropertyName("short_text")]
        public string ShortText;

        [SerializationPropertyName("url")]
        public string Url;

        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("classification")]
        public string Classification;

        [SerializationPropertyName("min_price")]
        public int MinPrice;

        [SerializationPropertyName("title")]
        public string Title;

        [SerializationPropertyName("type")]
        public string Type;

        [SerializationPropertyName("has_demo")]
        public bool HasDemo;

        [SerializationPropertyName("still_cover_url")]
        public string StillCoverUrl;
    }

    public class ItchSearchResult
    {
        [SerializationPropertyName("per_page")]
        public int PerPage;

        [SerializationPropertyName("page")]
        public int Page;

        [SerializationPropertyName("games")]
        public List<SearchedGame> Games;
    }
}
