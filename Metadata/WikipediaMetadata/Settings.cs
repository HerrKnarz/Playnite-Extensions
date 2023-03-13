using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    public class Settings : ObservableObject
    {
        private DateToUse dateToUse = DateToUse.Earliest;
        private RatingToUse ratingToUse = RatingToUse.Average;
        private bool advancedSearchResultSorting = true;
        private bool arcadeSystemAsPlatform = false;
        private bool removeDescriptionLinks = false;
        private bool descriptionOverviewOnly = false;
        private ObservableCollection<string> sectionsToRemove;
        private ObservableCollection<TagSetting> tagSettings;

        public DateToUse DateToUse { get => dateToUse; set => SetValue(ref dateToUse, value); }
        public RatingToUse RatingToUse { get => ratingToUse; set => SetValue(ref ratingToUse, value); }
        public bool AdvancedSearchResultSorting { get => advancedSearchResultSorting; set => SetValue(ref advancedSearchResultSorting, value); }
        public bool ArcadeSystemAsPlatform { get => arcadeSystemAsPlatform; set => SetValue(ref arcadeSystemAsPlatform, value); }
        public bool RemoveDescriptionLinks { get => removeDescriptionLinks; set => SetValue(ref removeDescriptionLinks, value); }
        public bool DescriptionOverviewOnly { get => descriptionOverviewOnly; set => SetValue(ref descriptionOverviewOnly, value); }
        public ObservableCollection<string> SectionsToRemove { get => sectionsToRemove; set => SetValue(ref sectionsToRemove, value); }
        public ObservableCollection<TagSetting> TagSettings { get => tagSettings; set => SetValue(ref tagSettings, value); }
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
        private Settings EditingClone { get; set; }

        private Settings settings;
        public Settings Settings
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
            { DateToUse.Earliest, ResourceProvider.GetString("LOCWikipediaMetadataSettingsDateEarliest") },
            { DateToUse.Latest, ResourceProvider.GetString("LOCWikipediaMetadataSettingsDateLatest") },
            { DateToUse.First, ResourceProvider.GetString("LOCWikipediaMetadataSettingsDateFirst") },
        };

        public Dictionary<RatingToUse, string> RatingToUseModes { get; } = new Dictionary<RatingToUse, string>
        {
            { RatingToUse.Lowest, ResourceProvider.GetString("LOCWikipediaMetadataSettingsRatingLowest") },
            { RatingToUse.Highest, ResourceProvider.GetString("LOCWikipediaMetadataSettingsRatingHighest") },
            { RatingToUse.Average, ResourceProvider.GetString("LOCWikipediaMetadataSettingsRatingAverage") },
        };

        public RelayCommand AddSectionCommand
        {
            get => new RelayCommand(() =>
            {
                string value = API.Instance.Dialogs.SelectString("", ResourceProvider.GetString("LOCWikipediaMetadataSettingsAddValue"), "").SelectedString;

                Settings.SectionsToRemove.AddMissing(value);
                Settings.SectionsToRemove = new ObservableCollection<string>(Settings.SectionsToRemove.OrderBy(x => x));
            });
        }

        public RelayCommand<IList<object>> RemoveSectionCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (string item in items.ToList().Cast<string>())
                {
                    Settings.SectionsToRemove.Remove(item);
                }
            }, (items) => items != null && items.Count > 0);
        }

        public WikipediaMetadataSettingsViewModel(WikipediaMetadata plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            Settings savedSettings = plugin.LoadPluginSettings<Settings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new Settings();
            }

            if (Settings.SectionsToRemove == null)
            {
                Settings.SectionsToRemove = new ObservableCollection<string>();
            }
            else
            {
                Settings.SectionsToRemove = new ObservableCollection<string>(Settings.SectionsToRemove.OrderBy(x => x));
            }

            if (Settings.TagSettings == null)
            {
                Settings.TagSettings = new ObservableCollection<TagSetting>
                {
                    new TagSetting()
                    {
                        IsChecked = true,
                        Name = "Arcade system",
                        Prefix = "[Arcade System]"
                    },
                    new TagSetting()
                    {
                        IsChecked = true,
                        Name = "Engine",
                        Prefix = "[Game Engine]"
                    },
                    new TagSetting()
                    {
                        IsChecked = true,
                        Name = "Director",
                        Prefix = "[People] director:"
                    },
                    new TagSetting()
                    {
                        IsChecked = true,
                        Name = "Producer",
                        Prefix = "[People] producer:"
                    },
                    new TagSetting()
                    {
                        IsChecked = true,
                        Name = "Designer",
                        Prefix = "[People] designer:"
                    },
                    new TagSetting()
                    {
                        IsChecked = true,
                        Name = "Programmer",
                        Prefix = "[People] programmer:"
                    },
                    new TagSetting()
                    {
                        IsChecked = true,
                        Name = "Artist",
                        Prefix = "[People] artist:"
                    },
                    new TagSetting()
                    {
                        IsChecked = true,
                        Name = "Writer",
                        Prefix = "[People] writer:"
                    },
                    new TagSetting()
                    {
                        IsChecked = true,
                        Name = "Composer",
                        Prefix = "[People] composer:"
                    },
                };
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