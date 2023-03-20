using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    public class WikipediaMetadataSettingsViewModel : ObservableObject, ISettings
    {
        private readonly WikipediaMetadata _plugin;
        private PluginSettings _settings;

        private PluginSettings EditingClone { get; set; }

        public PluginSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
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
            // Injecting your _plugin instance is required for Save/Load method because Playnite saves data to a location based on what _plugin requested the operation.
            this._plugin = plugin;

            // Load saved _settings.
            PluginSettings savedSettings = plugin.LoadPluginSettings<PluginSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            Settings = savedSettings ?? new PluginSettings();

            if (Settings.SectionsToRemove is null)
            {
                Settings.SectionsToRemove = new ObservableCollection<string>();
            }
            else
            {
                Settings.SectionsToRemove = new ObservableCollection<string>(Settings.SectionsToRemove.OrderBy(x => x));
            }

            if (Settings.TagSettings is null)
            {
                Settings.PopulateTagSettings();
            }
            // Hotfix to a bug that duplicated the tag _settings in version 1.3 and 1.4
            else if (Settings.TagSettings.Count > 9)
            {
                while (Settings.TagSettings.Count > 9)
                {
                    Settings.TagSettings.RemoveAt(9);
                }
            }
        }

        public void BeginEdit()
        {
            // Code executed when _settings view is opened and user starts editing values.
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
            // This method should save _settings made to Option1 and ArcadeSystemAsPlatform.
            _plugin.SavePluginSettings(Settings);
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