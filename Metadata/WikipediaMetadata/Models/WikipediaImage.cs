using Newtonsoft.Json;
using System.Collections.Generic;

namespace WikipediaMetadata.Models
{
    /// Contains the classes needed to fetch images from a Wikipedia page.
    public class Normalized
    {
        [JsonProperty("fromencoded")]
        public bool Fromencoded;

        [JsonProperty("from")]
        public string From;

        [JsonProperty("to")]
        public string To;
    }

    public class Original
    {
        [JsonProperty("source")]
        public string Source;

        [JsonProperty("width")]
        public int Width;

        [JsonProperty("height")]
        public int Height;
    }

    public class ImagePage
    {
        [JsonProperty("pageid")]
        public int Pageid;

        [JsonProperty("ns")]
        public int Ns;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("original")]
        public Original Original;

        [JsonProperty("terms")]
        public Terms Terms;
    }

    public class Query
    {
        [JsonProperty("normalized")]
        public List<Normalized> Normalized;

        [JsonProperty("pages")]
        public List<ImagePage> Pages;
    }

    public class WikipediaImage
    {
        [JsonProperty("batchcomplete")]
        public bool Batchcomplete;

        [JsonProperty("query")]
        public Query Query;
    }

    public class Terms
    {
        [JsonProperty("label")]
        public List<string> Label;

        [JsonProperty("description")]
        public List<string> Description;
    }
}