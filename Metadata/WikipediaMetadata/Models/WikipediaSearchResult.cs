using KNARZhelper;
using Playnite.SDK.Data;
using System.Collections.Generic;

namespace WikipediaMetadata.Models
{
    public class Page
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("key")]
        public string Key;

        [DontSerialize]
        public string KeyMatch { get => Key.RemoveSpecialChars().ToLower().Replace(" ", ""); }

        [SerializationPropertyName("title")]
        public string Title;

        [SerializationPropertyName("excerpt")]
        public string Excerpt;

        [SerializationPropertyName("matched_title")]
        public object MatchedTitle;

        [SerializationPropertyName("description")]
        public string Description;

        [SerializationPropertyName("thumbnail")]
        public Thumbnail Thumbnail;
    }

    public class WikipediaSearchResult
    {
        [SerializationPropertyName("pages")]
        public List<Page> Pages;
    }

    public class Thumbnail
    {
        [SerializationPropertyName("mimetype")]
        public string Mimetype;

        [SerializationPropertyName("size")]
        public object Size;

        [SerializationPropertyName("width")]
        public int Width;

        [SerializationPropertyName("height")]
        public int Height;

        [SerializationPropertyName("duration")]
        public object Duration;

        [SerializationPropertyName("url")]
        public string Url;
    }


}
