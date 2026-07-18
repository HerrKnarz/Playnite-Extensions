using LinkUtilities.Helper;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace LinkUtilities.Models;

/// <summary>
/// Pattern to find the right link name for a given URL and link title combination
/// </summary>
public class LinkNamePattern
{
    /// <summary>
    /// Name to use for the link if the patterns match
    /// </summary>
    [JsonPropertyName("linkName")]
    public string? LinkName { get; set; }

    /// <summary>
    /// If true the LinkName is a regular expression. If false it's a wildcard pattern where * means
    /// zero or more characters and ? means exactly one character.
    /// </summary>
    [JsonPropertyName("linkNameIsRegex")]
    public bool LinkNameIsRegex { get; set; } = false;

    /// <summary>
    /// Identifier of the LinkType to assign when ActionType is AssignLinkType or to check when
    /// ActionType is RemoveLink. Can be null if ActionType is RemoveLink.
    /// </summary>
    [JsonPropertyName("linkTypeIdentifier")]
    public string? LinkTypeIdentifier { get; set; }

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
    public string NameRegEx => LinkNameIsRegex ? NamePattern ?? string.Empty : ParseHelper.WildCardToRegular(NamePattern);

    /// <summary>
    /// If true only one of both patterns has to match. If false both have to match.
    /// </summary>
    [JsonPropertyName("partialMatch")]
    public bool PartialMatch { get; set; } = false;

    /// <summary>
    /// Pattern the URL has to match. Can contain wildcards * (zero or more characters) or ?
    /// (exactly one character).
    /// </summary>
    [JsonPropertyName("urlPattern")]
    public string? UrlPattern { get; set; }

    /// <summary>
    /// If true the UrlPattern is a regular expression. If false it's a wildcard pattern where *
    /// means zero or more characters and ? means exactly one character.
    /// </summary>
    [JsonPropertyName("urlPatternIsRegex")]
    public bool UrlPatternIsRegex { get; set; } = false;

    /// <summary>
    /// Regular expression of the UrlPattern
    /// </summary>
    [JsonIgnore]
    public string UrlRegEx => UrlPatternIsRegex ? UrlPattern ?? string.Empty : ParseHelper.WildCardToRegular(UrlPattern);

    public bool LinkMatch(string linkName, string linkUrl, string? linkTypeIdentifier = null, PatternMatchModes matchMode = PatternMatchModes.MatchByUrlAndName)
    {
        return matchMode switch
        {
            PatternMatchModes.MatchByUrl => !string.IsNullOrWhiteSpace(UrlPattern) && Regex.IsMatch(linkUrl, UrlRegEx),
            PatternMatchModes.MatchByName => !string.IsNullOrWhiteSpace(NamePattern) && Regex.IsMatch(linkName, NameRegEx),
            PatternMatchModes.MatchByUrlAndName => PartialMatch
                ? (!string.IsNullOrWhiteSpace(NamePattern) && Regex.IsMatch(linkName, NameRegEx)) ||
                    (!string.IsNullOrWhiteSpace(UrlPattern) && Regex.IsMatch(linkUrl, UrlRegEx))
                : (string.IsNullOrWhiteSpace(NamePattern) || Regex.IsMatch(linkName, NameRegEx)) &&
                    (string.IsNullOrWhiteSpace(UrlPattern) || Regex.IsMatch(linkUrl, UrlRegEx)),
            PatternMatchModes.MatchByType => !string.IsNullOrWhiteSpace(LinkTypeIdentifier) &&
                LinkTypeIdentifier == linkTypeIdentifier,
            PatternMatchModes.MatchByUrlAndType =>
                (string.IsNullOrWhiteSpace(LinkTypeIdentifier) || LinkTypeIdentifier == linkTypeIdentifier) &&
                (string.IsNullOrWhiteSpace(UrlPattern) || Regex.IsMatch(linkUrl, UrlRegEx)),
            _ => false,
        };
    }
}