using Newtonsoft.Json;
using System.Collections.Generic;

namespace LinkUtilities.Models.IsThereAnyDeal
{
    public class Data
    {
        [JsonProperty("results")]
        public List<Result> Results;

        [JsonProperty("urls")]
        public Urls Urls;
    }

    public class Result
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("plain")]
        public string Plain;

        [JsonProperty("title")]
        public string Title;
    }

    public class IsThereAnyDealSearchResult
    {
        [JsonProperty("data")]
        public Data Data;
    }

    public class Urls
    {
        [JsonProperty("search")]
        public string Search;
    }
}