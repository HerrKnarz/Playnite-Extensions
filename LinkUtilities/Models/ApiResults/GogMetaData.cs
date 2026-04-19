using System.Text.Json.Serialization;

namespace LinkUtilities.Models.ApiResults;

public class GogMetaData
{
    [JsonPropertyName("slug")]
    public string? Slug { get; set; }
}