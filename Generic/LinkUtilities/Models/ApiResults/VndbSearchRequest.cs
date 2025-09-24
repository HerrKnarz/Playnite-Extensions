using Newtonsoft.Json;
using System.Collections.Generic;

namespace LinkUtilities.Models.ApiResults
{
    public class VndbSearchRequest
    {
        [JsonProperty("filters")]
        public List<string> Filters;

        [JsonProperty("fields")]
        public string Fields;

        [JsonProperty("results")]
        public int Results;

        [JsonProperty("sort")]
        public string Sort;
    }
}
