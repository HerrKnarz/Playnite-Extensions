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
        private ObservableCollection<string> sectionsToRemove = new ObservableCollection<string>();
        private ObservableCollection<TagSetting> tagSettings;

        public DateToUse DateToUse { get => dateToUse; set => SetValue(ref dateToUse, value); }
        public RatingToUse RatingToUse { get => ratingToUse; set => SetValue(ref ratingToUse, value); }
        public bool AdvancedSearchResultSorting { get => advancedSearchResultSorting; set => SetValue(ref advancedSearchResultSorting, value); }
        public bool ArcadeSystemAsPlatform { get => arcadeSystemAsPlatform; set => SetValue(ref arcadeSystemAsPlatform, value); }
        public bool RemoveDescriptionLinks { get => removeDescriptionLinks; set => SetValue(ref removeDescriptionLinks, value); }
        public bool DescriptionOverviewOnly { get => descriptionOverviewOnly; set => SetValue(ref descriptionOverviewOnly, value); }
        public ObservableCollection<string> SectionsToRemove { get => sectionsToRemove; set => SetValue(ref sectionsToRemove, value); }
        public ObservableCollection<TagSetting> TagSettings { get => tagSettings; set => SetValue(ref tagSettings, value); }

        public void PopulateTagSettings()
        {
            if (tagSettings == null)
            {
                tagSettings = new ObservableCollection<TagSetting>
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
    }
}
