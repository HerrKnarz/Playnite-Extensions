using Newtonsoft.Json;

// Contains all the classes needed to deserialize the JSON fetched from the itch.io api.
namespace LinkUtilities.Models.Itch
{
    public class Embed
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("fullscreen")]
        public bool Fullscreen { get; set; }
    }

    public class Game
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("published_at")]
        public string PublishedAt { get; set; }

        [JsonProperty("embed")]
        public Embed Embed { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("classification")]
        public string Classification { get; set; }

        [JsonProperty("traits")]
        public Traits Traits { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("cover_url")]
        public string CoverUrl { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("min_price")]
        public int MinPrice { get; set; }

        [JsonProperty("short_text")]
        public string ShortText { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class ItchMetaData
    {
        [JsonProperty("game")]
        public Game Game { get; set; }
    }

    public class Traits
    {
    }

    public class User
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("cover_url")]
        public string CoverUrl { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
