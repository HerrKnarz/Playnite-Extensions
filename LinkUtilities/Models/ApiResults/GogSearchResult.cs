using System.Text.Json.Serialization;

namespace LinkUtilities.Models.ApiResults;

public class GogSearchResult
{
    [JsonPropertyName("productCount")]
    public int ProductCount { get; set; }

    [JsonPropertyName("products")]
    public List<Product>? Products { get; set; }
}

public class Product
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("releaseDate")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("storeLink")]
    public string? StoreLink { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}