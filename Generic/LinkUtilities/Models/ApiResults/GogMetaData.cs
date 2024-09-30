using Newtonsoft.Json;

// Contains all the classes needed to deserialize the JSON fetched from the gog api.
namespace LinkUtilities.Models.ApiResults
{
    public class GogMetaData
    {
        [JsonProperty("slug")]
        public string Slug;
    }
}