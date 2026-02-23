using KNARZhelper;
using Newtonsoft.Json;
using Playnite.SDK.Data;
using System.Collections.Generic;

namespace WikipediaMetadata.Models;

/// Contains all the classes needed for the JSON result of a search query.
public class Page
{
    [JsonProperty("description")]
    public string Description;

    [JsonProperty("excerpt")]
    public string Excerpt;

    [JsonProperty("id")]
    public int Id;

    [JsonProperty("key")]
    public string Key;

    [JsonProperty("matched_title")]
    public object MatchedTitle;

    [JsonProperty("thumbnail")]
    public Thumbnail Thumbnail;

    [JsonProperty("title")]
    public string Title;

    [DontSerialize]
    public string KeyMatch => Key.NormalizeSearchTerm();
}

public class WikipediaSearchResult
{
    [JsonProperty("pages")]
    public List<Page> Pages;
}

public class Thumbnail
{
    [JsonProperty("duration")]
    public object Duration;

    [JsonProperty("height")]
    public int Height;

    [JsonProperty("mimetype")]
    public string Mimetype;

    [JsonProperty("size")]
    public object Size;

    [JsonProperty("url")]
    public string Url;

    [JsonProperty("width")]
    public int Width;
}
