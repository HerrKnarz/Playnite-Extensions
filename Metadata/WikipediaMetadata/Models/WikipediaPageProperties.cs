using Newtonsoft.Json;
using System.Collections.Generic;

namespace WikipediaMetadata.Models;

/// Contains the classes needed to fetch images from a Wikipedia page.
public class NormalizedPageTitle
{
    [JsonProperty("fromencoded")]
    public bool FromEncoded;

    [JsonProperty("from")]
    public string From;

    [JsonProperty("to")]
    public string To;
}

public class OriginalImage
{
    [JsonProperty("source")]
    public string Source;

    [JsonProperty("width")]
    public int Width;

    [JsonProperty("height")]
    public int Height;
}

public class PropertiesPage
{
    [JsonProperty("pageid")]
    public int PageId;

    [JsonProperty("ns")]
    public int Namespace;

    [JsonProperty("title")]
    public string Title;

    [JsonProperty("original")]
    public OriginalImage Original;

    [JsonProperty("terms")]
    public Terms Terms;

    [JsonProperty("categories")]
    public List<WikipediaCategory> Categories;
}

public class PagePropertiesQuery
{
    [JsonProperty("normalized")]
    public List<NormalizedPageTitle> Normalized;

    [JsonProperty("pages")]
    public List<PropertiesPage> Pages;
}

public class PagePropertiesResponse
{
    [JsonProperty("query")]
    public PagePropertiesQuery Query;
}

public class Terms
{
    [JsonProperty("label")]
    public List<string> Label;

    [JsonProperty("description")]
    public List<string> Description;
}

public class WikipediaCategory
{
    [JsonProperty("ns")]
    public int Namespace;

    [JsonProperty("title")]
    public string Title;
}
