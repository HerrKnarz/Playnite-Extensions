using System.Text.Json.Serialization;
using System.Windows;

namespace LinkUtilities.Models;

/// <summary>
/// Contains all needed settings for a link
/// </summary>
public class LinkSourceSetting
{
    /// <summary>
    /// API key needed for the specific website. Can be empty if no key is needed.
    /// </summary>
    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// Specifies if the link will be automatically added when clicking "all configured websites".
    /// Null indicates, that the link has no add functionality.
    /// </summary>
    [JsonPropertyName("isAddable")]
    public bool? IsAddable { get; set; }

    /// <summary>
    /// Visibility property for the "Add link" check box in the settings window. Will be hidden, if
    /// the site doesn't have an add functionality.
    /// </summary>
    [JsonIgnore]
    public Visibility IsAddableVisible => IsAddable != null ? Visibility.Visible : Visibility.Hidden;

    /// <summary>
    /// Visibility property for the "API key" text box in the settings window. Will be hidden, if
    /// the site doesn't need an API key.
    /// </summary>
    [JsonIgnore]
    public Visibility IsApiKeyVisible => NeedsApiKey ? Visibility.Visible : Visibility.Hidden;

    /// <summary>
    /// Specifies, if the link source is a custom one added by the user. These won't show in the
    /// settings for example.
    /// </summary>
    [JsonIgnore]
    public bool IsCustomSource { get; set; } = false;

    /// <summary>
    /// Specifies if the link will be automatically searched when clicking "all configured
    /// websites". Null indicates, that the link has no search functionality.
    /// </summary>
    [JsonPropertyName("isSearchable")]
    public bool? IsSearchable { get; set; }

    /// <summary>
    /// Visibility property for the "Search link" check box in the settings window. Will be hidden,
    /// if the site doesn't have a search functionality.
    /// </summary>
    [JsonIgnore]
    public Visibility IsSearchableVisible => IsSearchable != null ? Visibility.Visible : Visibility.Hidden;

    /// <summary>
    /// Name of the link. Is a bit redundant, but it's needed to find the right settings from the
    /// config file.
    /// </summary>
    [JsonPropertyName("linkName")]
    public string? LinkName { get; set; }

    // NEXT: Check if it isn't smarter to use the TypeID here instead or in addition to the name.

    /// <summary>
    /// Indicates if the websites needs an APIKey to function
    /// </summary>
    [JsonIgnore]
    public bool NeedsApiKey { get; set; }

    /// <summary>
    /// Indicates if the link source will be visible in the game menu.
    /// </summary>
    [JsonPropertyName("showInMenus")]
    public bool ShowInMenus { get; set; }
}