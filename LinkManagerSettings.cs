using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;

namespace LinkManager
{
    /// <summary>
    /// Contains all settings for the extension
    /// </summary>
    public class LinkManagerSettings : ObservableObject
    {
        private string itchApiKey = string.Empty;

        /// <summary>
        /// API key used to get game information from itch.io
        /// </summary>
        public string ItchApiKey { get => itchApiKey; set => SetValue(ref itchApiKey, value); }
    }

    public class LinkManagerSettingsViewModel : ObservableObject, ISettings
    {
        private readonly LinkManager plugin;
        private LinkManagerSettings EditingClone { get; set; }

        private LinkManagerSettings settings;
        public LinkManagerSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public LinkManagerSettingsViewModel(LinkManager plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            LinkManagerSettings savedSettings = plugin.LoadPluginSettings<LinkManagerSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new LinkManagerSettings();
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