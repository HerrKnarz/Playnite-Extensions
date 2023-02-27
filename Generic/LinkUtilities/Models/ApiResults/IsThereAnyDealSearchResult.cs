using Playnite.SDK.Data;
using System.Collections.Generic;

namespace LinkUtilities.Models.IsThereAnyDeal
{
    public class Data
    {
        [SerializationPropertyName("results")]
        public List<Result> Results;

        [SerializationPropertyName("urls")]
        public Urls Urls;
    }

    public class Result
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("plain")]
        public string Plain;

        [SerializationPropertyName("title")]
        public string Title;
    }

    public class IsThereAnyDealSearchResult
    {
        [SerializationPropertyName("data")]
        public Data Data;
    }

    public class Urls
    {
        [SerializationPropertyName("search")]
        public string Search;
    }
}
