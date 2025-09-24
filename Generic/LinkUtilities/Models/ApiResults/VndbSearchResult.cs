using Newtonsoft.Json;
using System.Collections.Generic;

namespace LinkUtilities.Models.ApiResults
{
    public class VndbResult
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("released")]
        public string Released;

        [JsonProperty("title")]
        public string Title;
    }

    public class VndbSearchResult
    {
        [JsonProperty("more")]
        public bool More;

        [JsonProperty("results")]
        public List<VndbResult> Results;
    }
}
