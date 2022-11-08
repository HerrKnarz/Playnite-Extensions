using Newtonsoft.Json;
using System.Windows;

namespace LinkUtilities.Models
{
    /// <summary>
    /// Contains all needed settings for a link
    /// </summary>
    public class LinkSourceSettings
    {
        /// <summary>
        /// Name of the link. Is a bit redundant, but it's needed to find the right settings from the config file.
        /// </summary>
        [JsonProperty("linkName")]
        public string LinkName { get; set; }

        /// <summary>
        /// Specifys if the link will be automatically added when clicking "all configured websites". Null indicates, that the
        /// link has no add functionality.
        /// </summary>
        [JsonProperty("isAddable")]
        public bool? IsAddable { get; set; }

        /// <summary>
        /// Specifys if the link will be automatically searched when clicking "all configured websites". Null indicates, that the
        /// link has no search functionality.
        /// </summary>
        [JsonProperty("isSearchable")]
        public bool? IsSearchable { get; set; }

        /// <summary>
        /// Indicates if the link source will be visible in the game menu.
        /// </summary>
        [JsonProperty("showInMenus")]
        public bool ShowInMenus { get; set; }

        /// <summary>
        /// API key needed for the specific website. Can be empty if no key is needed.
        /// </summary>
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        /// <summary>
        /// Indicates if the websites needs an APIKey to function
        /// </summary>
        [JsonIgnore]
        public bool NeedsApiKey { get; set; }

        /// <summary>
        /// Visibility property for the "Add link" check box in the settings window. Will be hidden, if the site doesn't have an
        /// add functionality.
        /// </summary>
        [JsonIgnore]
        public Visibility IsAddableVisible { get => (IsAddable != null) ? Visibility.Visible : Visibility.Hidden; }

        /// <summary>
        /// Visibility property for the "Search link" check box in the settings window. Will be hidden, if the site doesn't have
        /// a search functionality.
        /// </summary>
        [JsonIgnore]
        public Visibility IsSearchableVisible { get => (IsSearchable != null) ? Visibility.Visible : Visibility.Hidden; }

        /// <summary>
        /// Visibility property for the "API key" text box in the settings window. Will be hidden, if the site doesn't need an
        /// API key.
        /// </summary>
        [JsonIgnore]
        public Visibility IsApiKeyVisible { get => (NeedsApiKey) ? Visibility.Visible : Visibility.Hidden; }
    }
}
