using Newtonsoft.Json;
using System.Collections.Generic;

namespace ScreenshotUtilitiesSteamProvider
{
    public class AppDetails
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("screenshots")]
        public List<Screenshot> Screenshots { get; set; }
    }

    public class SteamAppDetails : Dictionary<string, AppDetails>
    {
    }

    public class Screenshot
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("path_thumbnail")]
        public string PathThumbnail { get; set; }

        [JsonProperty("path_full")]
        public string PathFull { get; set; }
    }
}
