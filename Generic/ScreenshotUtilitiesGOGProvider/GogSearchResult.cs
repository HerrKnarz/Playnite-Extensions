using Newtonsoft.Json;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the gog catalog URL.
namespace ScreenshotUtilitiesGOGProvider
{
    public class GogSearchResult
    {
        [JsonProperty("products")]
        public List<Product> Products;
    }

    public class Product
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("releaseDate")]
        public string ReleaseDate;

        [JsonProperty("title")]
        public string Title;
    }
}