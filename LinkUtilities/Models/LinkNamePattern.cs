using LinkUtilities.Helper;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace LinkUtilities.Models;

/// <summary>
/// Pattern to find the right link name for a given URL and link title combination
/// </summary>
public class LinkNamePattern
{
    //NEXT: Remake with LinkType in mind!
    /// <summary>
    /// Name to use for the link if the patterns match
    /// </summary>
    [JsonPropertyName("linkName")]
    public string? LinkName { get; set; }

    /// <summary>
    /// Pattern the link title has to match. Can contain wildcards * (zero or more characters) or ?
    /// (exactly one character).
    /// </summary>
    [JsonPropertyName("namePattern")]
    public string? NamePattern { get; set; }

    /// <summary>
    /// Regular expression of the NamePattern
    /// </summary>
    [JsonIgnore]
    public string NameRegEx => ParseHelper.WildCardToRegular(NamePattern);

    /// <summary>
    /// If true only one of both patterns has to match. If false both have to match.
    /// </summary>
    [JsonPropertyName("partialMatch")]
    public bool PartialMatch { get; set; } = false;

    /// <summary>
    /// Position used to sort the patterns
    /// </summary>
    [JsonPropertyName("position")]
    public int? Position { get; set; }

    /// <summary>
    /// Pattern the URL has to match. Can contain wildcards * (zero or more characters) or ?
    /// (exactly one character).
    /// </summary>
    [JsonPropertyName("urlPattern")]
    public string? UrlPattern { get; set; }

    /// <summary>
    /// Regular expression of the UrlPattern
    /// </summary>
    [JsonIgnore]
    public string UrlRegEx => ParseHelper.WildCardToRegular(UrlPattern);

    public bool LinkMatch(string linkName, string linkUrl, bool overridePartialMatch = false)
        => overridePartialMatch || PartialMatch
            ? (!string.IsNullOrWhiteSpace(NamePattern) && Regex.IsMatch(linkName, NameRegEx)) ||
              (!string.IsNullOrWhiteSpace(UrlPattern) && Regex.IsMatch(linkUrl, UrlRegEx))
            : (string.IsNullOrWhiteSpace(NamePattern) || Regex.IsMatch(linkName, NameRegEx)) &&
              (string.IsNullOrWhiteSpace(UrlPattern) || Regex.IsMatch(linkUrl, UrlRegEx));
}