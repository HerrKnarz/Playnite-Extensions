using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;

namespace GGDealsWishlist
{
    public class GGDealsWishlistSettings : ObservableObject
    {
        private string wishlistUrl = string.Empty;

        public string WishlistUrl { get => wishlistUrl; set => SetValue(ref wishlistUrl, value); }

        //TODO: Add option to set the game as installed or not
        //TODO: Rework settings XAML and add descriptions.
    }

    public class GGDealsWishlistSettingsViewModel : ObservableObject, ISettings
    {
        private readonly GGDealsWishlist plugin;
        private GGDealsWishlistSettings settings;

        public GGDealsWishlistSettingsViewModel(GGDealsWishlist plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite
            // saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<GGDealsWishlistSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            Settings = savedSettings ?? new GGDealsWishlistSettings();
        }

        public GGDealsWishlistSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private GGDealsWishlistSettings EditingClone { get; set; }

        public void BeginEdit() =>
            // Code executed when settings view is opened and user starts editing values.
            EditingClone = Serialization.GetClone(Settings);

        public void CancelEdit() =>
            // Code executed when user decides to cancel any changes made since BeginEdit was
            // called. This method should revert any changes made to Option1 and Option2.
            Settings = EditingClone;

        public void EndEdit() =>
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);

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
