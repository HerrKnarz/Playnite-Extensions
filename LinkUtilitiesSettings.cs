using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;

namespace LinkUtilities
{
    /// <summary>
    /// Contains all settings for the extension
    /// </summary>
    public class LinkUtilitiesSettings : ObservableObject
    {
        private bool sortAfterChange = false;
        private string itchApiKey = string.Empty;

        /// <summary>
        /// sets whether the Links shall be sorted after a game is updated in the database
        /// </summary>
        public bool SortAfterChange { get => sortAfterChange; set => SetValue(ref sortAfterChange, value); }

        /// <summary>
        /// API key used to get game information from itch.io
        /// </summary>
        public string ItchApiKey { get => itchApiKey; set => SetValue(ref itchApiKey, value); }
    }

    public class LinkUtilitiesSettingsViewModel : ObservableObject, ISettings
    {
        private readonly LinkUtilities plugin;
        private LinkUtilitiesSettings EditingClone { get; set; }

        private LinkUtilitiesSettings settings;
        public LinkUtilitiesSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public LinkUtilitiesSettingsViewModel(LinkUtilities plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            LinkUtilitiesSettings savedSettings = plugin.LoadPluginSettings<LinkUtilitiesSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new LinkUtilitiesSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = EditingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}