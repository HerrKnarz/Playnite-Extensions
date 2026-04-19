using System.Text.Json.Serialization;

namespace LinkUtilities.Models.ApiResults;

public class GameFaqsSearchResult
{
    [JsonPropertyName("date_released")]
    public string? DateReleased { get; set; }

    [JsonPropertyName("game_id")]
    public string? GameId { get; set; }

    [JsonPropertyName("game_name")]
    public string? GameName { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}