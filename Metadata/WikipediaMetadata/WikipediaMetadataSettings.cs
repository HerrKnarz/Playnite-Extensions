using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;

namespace WikipediaMetadata
{
    public class WikipediaMetadataSettings : ObservableObject
    {
        private DateToUse dateToUse = DateToUse.Earliest;
        private RatingToUse ratingToUse = RatingToUse.Average;
        private bool advancedSearchResultSorting = true;
        private bool arcadeSystemAsPlatform = false;

        public DateToUse DateToUse { get => dateToUse; set => SetValue(ref dateToUse, value); }
        public RatingToUse RatingToUse { get => ratingToUse; set => SetValue(ref ratingToUse, value); }

        public bool AdvancedSearchResultSorting { get => advancedSearchResultSorting; set => SetValue(ref advancedSearchResultSorting, value); }
        public bool ArcadeSystemAsPlatform { get => arcadeSystemAsPlatform; set => SetValue(ref arcadeSystemAsPlatform, value); }
    }

    public enum DateToUse
    {
        Earliest,
        Latest,
        First,
    }

    public enum RatingToUse
    {
        Lowest,
        Highest,
        Average,
    }


    public class WikipediaMetadataSettingsViewModel : ObservableObject, ISettings
    {
        private readonly WikipediaMetadata plugin;
        private WikipediaMetadataSettings EditingClone { get; set; }

        private WikipediaMetadataSettings settings;
        public WikipediaMetadataSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<DateToUse, string> DateToUseModes { get; } = new Dictionary<DateToUse, string>
        {
            { DateToUse.Earliest, "Earliest date" },
            { DateToUse.Latest, "Latest date" },
            { DateToUse.First, "First date in the list" },
        };

        public Dictionary<RatingToUse, string> RatingToUseModes { get; } = new Dictionary<RatingToUse, string>
        {
            { RatingToUse.Lowest, "Lowest rating" },
            { RatingToUse.Highest, "Highest rating" },
            { RatingToUse.Average, "Average of all ratings" },
        };

        public WikipediaMetadataSettingsViewModel(WikipediaMetadata plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            WikipediaMetadataSettings savedSettings = plugin.LoadPluginSettings<WikipediaMetadataSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new WikipediaMetadataSettings();
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
            // This method should revert any changes made to Option1 and ArcadeSystemAsPlatform.
            Settings = EditingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and ArcadeSystemAsPlatform.
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