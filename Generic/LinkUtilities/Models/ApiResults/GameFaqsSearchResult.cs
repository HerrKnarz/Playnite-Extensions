using Newtonsoft.Json;

namespace LinkUtilities.Models.ApiResults
{
    public class GameFaqsSearchResult
    {
        [JsonProperty("game_id")]
        public string GameId;

        [JsonProperty("game_name")]
        public string GameName;

        [JsonProperty("plats")]
        public string Plats;

        [JsonProperty("pcnt")]
        public string Pcnt;

        [JsonProperty("date_released")]
        public string DateReleased;

        [JsonProperty("has_guides")]
        public string HasGuides;

        [JsonProperty("has_cheats")]
        public string HasCheats;

        [JsonProperty("has_qna")]
        public string HasQna;

        [JsonProperty("has_reviews")]
        public string HasReviews;

        [JsonProperty("has_news")]
        public string HasNews;

        [JsonProperty("release_date")]
        public string ReleaseDate;

        [JsonProperty("product_name")]
        public string ProductName;

        [JsonProperty("gs_product_id")]
        public string GsProductId;

        [JsonProperty("platform_url")]
        public string PlatformUrl;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("board_url")]
        public string BoardUrl;

        [JsonProperty("pid")]
        public string Pid;

        [JsonProperty("row_num")]
        public int RowNum;

        [JsonProperty("product")]
        public bool Product;

        [JsonProperty("search_string")]
        public string SearchString;

        [JsonProperty("text")]
        public string Text;

        [JsonProperty("footer")]
        public bool? Footer;

        [JsonProperty("tokens")]
        public object Tokens;
    }
}
