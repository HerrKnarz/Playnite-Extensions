using KNARZhelper;
using Newtonsoft.Json;
using Playnite.SDK.Data;
using System.Collections.Generic;

/// Contains all the classes needed for the JOSN result of a search query.
namespace WikipediaMetadata.Models
{
    public class Page
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("key")]
        public string Key;

        [DontSerialize]
        public string KeyMatch { get => Key.RemoveSpecialChars().ToLower().Replace(" ", ""); }

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("excerpt")]
        public string Excerpt;

        [JsonProperty("matched_title")]
        public object MatchedTitle;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("thumbnail")]
        public Thumbnail Thumbnail;
    }

    public class WikipediaSearchResult
    {
        [JsonProperty("pages")]
        public List<Page> Pages;
    }

    public class Thumbnail
    {
        [JsonProperty("mimetype")]
        public string Mimetype;

        [JsonProperty("size")]
        public object Size;

        [JsonProperty("width")]
        public int Width;

        [JsonProperty("height")]
        public int Height;

        [JsonProperty("duration")]
        public object Duration;

        [JsonProperty("url")]
        public string Url;
    }


}
