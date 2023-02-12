using Playnite.SDK.Data;

// Contains all the classes needed to deserialize the JSON fetched from the itch.io api.
namespace LinkUtilities.Models.Itch
{
    public class Embed
    {
        [SerializationPropertyName("width")]
        public int Width { get; set; }

        [SerializationPropertyName("height")]
        public int Height { get; set; }

        [SerializationPropertyName("fullscreen")]
        public bool Fullscreen { get; set; }
    }

    public class Game
    {
        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("user")]
        public User User { get; set; }

        [SerializationPropertyName("published_at")]
        public string PublishedAt { get; set; }

        [SerializationPropertyName("embed")]
        public Embed Embed { get; set; }

        [SerializationPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [SerializationPropertyName("classification")]
        public string Classification { get; set; }

        [SerializationPropertyName("traits")]
        public Traits Traits { get; set; }

        [SerializationPropertyName("url")]
        public string Url { get; set; }

        [SerializationPropertyName("cover_url")]
        public string CoverUrl { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("min_price")]
        public int MinPrice { get; set; }

        [SerializationPropertyName("short_text")]
        public string ShortText { get; set; }

        [SerializationPropertyName("type")]
        public string Type { get; set; }
    }

    public class ItchMetaData
    {
        [SerializationPropertyName("game")]
        public Game Game { get; set; }
    }

    public class Traits
    {
    }

    public class User
    {
        [SerializationPropertyName("display_name")]
        public string DisplayName { get; set; }

        [SerializationPropertyName("cover_url")]
        public string CoverUrl { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("username")]
        public string Username { get; set; }

        [SerializationPropertyName("url")]
        public string Url { get; set; }
    }
}
