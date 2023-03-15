using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WikipediaMetadata.Models
{
    public class PluginSettings : ObservableObject
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
}
