using Playnite.SDK.Data;
using System.Windows;

namespace LinkUtilities.Models
{
    /// <summary>
    ///     Contains all needed settings for a link
    /// </summary>
    public class LinkSourceSetting
    {
        /// <summary>
        ///     API key needed for the specific website. Can be empty if no key is needed.
        /// </summary>
        [SerializationPropertyName("apiKey")]
        public string ApiKey { get; set; }

        /// <summary>
        ///     Specifies if the link will be automatically added when clicking "all configured websites". Null indicates, that the
        ///     link has no add functionality.
        /// </summary>
        [SerializationPropertyName("isAddable")]
        public bool? IsAddable { get; set; }

        /// <summary>
        ///     Visibility property for the "Add link" check box in the settings window. Will be hidden, if the site doesn't have
        ///     an
        ///     add functionality.
        /// </summary>
        [DontSerialize]
        public Visibility IsAddableVisible => IsAddable != null ? Visibility.Visible : Visibility.Hidden;

        /// <summary>
        ///     Visibility property for the "API key" text box in the settings window. Will be hidden, if the site doesn't need an
        ///     API key.
        /// </summary>
        [DontSerialize]
        public Visibility IsApiKeyVisible => NeedsApiKey ? Visibility.Visible : Visibility.Hidden;

        /// <summary>
        ///     Specifies, if the link source is a custom one added by the user. These won't show in the settings for example.
        /// </summary>
        [DontSerialize]
        public bool IsCustomSource { get; set; } = false;

        /// <summary>
        ///     Specifies if the link will be automatically searched when clicking "all configured websites". Null indicates, that
        ///     the
        ///     link has no search functionality.
        /// </summary>
        [SerializationPropertyName("isSearchable")]
        public bool? IsSearchable { get; set; }

        /// <summary>
        ///     Visibility property for the "Search link" check box in the settings window. Will be hidden, if the site doesn't
        ///     have
        ///     a search functionality.
        /// </summary>
        [DontSerialize]
        public Visibility IsSearchableVisible => IsSearchable != null ? Visibility.Visible : Visibility.Hidden;

        /// <summary>
        ///     Name of the link. Is a bit redundant, but it's needed to find the right settings from the config file.
        /// </summary>
        [SerializationPropertyName("linkName")]
        public string LinkName { get; set; }

        /// <summary>
        ///     Indicates if the websites needs an APIKey to function
        /// </summary>
        [DontSerialize]
        public bool NeedsApiKey { get; set; }

        /// <summary>
        ///     Indicates if the link source will be visible in the game menu.
        /// </summary>
        [SerializationPropertyName("showInMenus")]
        public bool ShowInMenus { get; set; }
    }
}