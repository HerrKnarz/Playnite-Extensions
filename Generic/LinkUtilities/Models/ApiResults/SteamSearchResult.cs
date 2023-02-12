using Playnite.SDK.Data;

// Contains the class to deserialize the steam search result.
namespace LinkUtilities.Models.Steam
{
    public class SteamSearchResult
    {
        [SerializationPropertyName("appid")]
        public string Appid;

        [SerializationPropertyName("name")]
        public string Name;

        [SerializationPropertyName("icon")]
        public string Icon;

        [SerializationPropertyName("logo")]
        public string Logo;
    }


}
