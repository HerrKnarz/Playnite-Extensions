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

        public WikipediaMetadataSettingsViewModel(WikipediaMetadata plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            _plugin = plugin;

            // LoadPluginSettings returns null if no saved data is available.
            Settings = plugin.LoadPluginSettings<PluginSettings>() ?? new PluginSettings();

            Settings.SectionsToRemove = Settings.SectionsToRemove is null
                ? new ObservableCollection<string>()
                : new ObservableCollection<string>(Settings.SectionsToRemove.OrderBy(x => x));

            if (Settings.TagSettings is null)
            {
                Settings.PopulateTagSettings();
            }
            // Hotfix to a bug that duplicated the tag settings in version 1.3 and 1.4
            else if (Settings.TagSettings.Count > 9)
            {
                while (Settings.TagSettings.Count > 9)
                {
                    Settings.TagSettings.RemoveAt(9);
                }
            }
        }

        public RelayCommand AddSectionCommand
            => new RelayCommand(() =>
            {
                var value = API.Instance.Dialogs.SelectString("", ResourceProvider.GetString("LOCWikipediaMetadataSettingsAddValue"), "").SelectedString;

                Settings.SectionsToRemove.AddMissing(value);
                Settings.SectionsToRemove = new ObservableCollection<string>(Settings.SectionsToRemove.OrderBy(x => x));
            });

        public Dictionary<DateToUse, string> DateToUseModes { get; } = new Dictionary<DateToUse, string>
        {
            { DateToUse.Earliest, ResourceProvider.GetString("LOCWikipediaMetadataSettingsDateEarliest") },
            { DateToUse.Latest, ResourceProvider.GetString("LOCWikipediaMetadataSettingsDateLatest") },
            { DateToUse.First, ResourceProvider.GetString("LOCWikipediaMetadataSettingsDateFirst") }
        };

        private PluginSettings EditingClone { get; set; }

        public Dictionary<RatingToUse, string> RatingToUseModes { get; } = new Dictionary<RatingToUse, string>
        {
            { RatingToUse.Lowest, ResourceProvider.GetString("LOCWikipediaMetadataSettingsRatingLowest") },
            { RatingToUse.Highest, ResourceProvider.GetString("LOCWikipediaMetadataSettingsRatingHighest") },
            { RatingToUse.Average, ResourceProvider.GetString("LOCWikipediaMetadataSettingsRatingAverage") }
        };

        public RelayCommand<IList<object>> RemoveSectionCommand => new RelayCommand<IList<object>>((items) =>
        {
            foreach (var item in items.ToList().Cast<string>())
            {
                Settings.SectionsToRemove.Remove(item);
            }
        }, (items) => items?.Any() ?? false);

        public PluginSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public void BeginEdit() => EditingClone = Serialization.GetClone(Settings);

        public void CancelEdit() => Settings = EditingClone;

        public void EndEdit() => _plugin.SavePluginSettings(Settings);

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}