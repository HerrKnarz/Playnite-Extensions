using System.Text.Json.Serialization;

namespace LinkUtilities.Models.ApiResults;

public class Catalog
{
    [JsonPropertyName("searchStore")]
    public SearchStore? SearchStore { get; set; }
}

public class Data
{
    [JsonPropertyName("Catalog")]
    public Catalog? Catalog { get; set; }
}

public class Element
{
    [JsonPropertyName("productSlug")]
    public string? ProductSlug { get; set; }

    [JsonPropertyName("seller")]
    public Seller? Seller { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

public class EpicSearchResult
{
    [JsonPropertyName("data")]
    public Data? Data { get; set; }
}

public class SearchStore
{
    [JsonPropertyName("elements")]
    public List<Element>? Elements { get; set; }
}

public class Seller
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}