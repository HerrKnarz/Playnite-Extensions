using Playnite.SDK.Data;
using System.Collections.Generic;

/// Contains the classes needed to fetch images from a Wikipedia page.
namespace WikipediaMetadata.Models
{
    public class Normalized
    {
        [SerializationPropertyName("fromencoded")]
        public bool Fromencoded;

        [SerializationPropertyName("from")]
        public string From;

        [SerializationPropertyName("to")]
        public string To;
    }

    public class Original
    {
        [SerializationPropertyName("source")]
        public string Source;

        [SerializationPropertyName("width")]
        public int Width;

        [SerializationPropertyName("height")]
        public int Height;
    }

    public class ImagePage
    {
        [SerializationPropertyName("pageid")]
        public int Pageid;

        [SerializationPropertyName("ns")]
        public int Ns;

        [SerializationPropertyName("title")]
        public string Title;

        [SerializationPropertyName("original")]
        public Original Original;

        [SerializationPropertyName("terms")]
        public Terms Terms;
    }

    public class Query
    {
        [SerializationPropertyName("normalized")]
        public List<Normalized> Normalized;

        [SerializationPropertyName("pages")]
        public List<ImagePage> Pages;
    }

    public class WikipediaImage
    {
        [SerializationPropertyName("batchcomplete")]
        public bool Batchcomplete;

        [SerializationPropertyName("query")]
        public Query Query;
    }

    public class Terms
    {
        [SerializationPropertyName("label")]
        public List<string> Label;

        [SerializationPropertyName("description")]
        public List<string> Description;
    }


}
