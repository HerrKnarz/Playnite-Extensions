using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WikipediaMetadata.Models;

public class PluginSettings : ObservableObject
{
    private bool _advancedSearchResultSorting = true;
    private bool _arcadeSystemAsPlatform;
    private DateToUse _dateToUse = DateToUse.Earliest;
    private bool _descriptionOverviewOnly;
    private RatingToUse _ratingToUse = RatingToUse.Average;
    private bool _removeDescriptionLinks;
    private ObservableCollection<string> _sectionsToRemove = [];
    private ObservableCollection<TagSetting> _tagSettings;

    public bool AdvancedSearchResultSorting
    {
        get => _advancedSearchResultSorting;
        set => SetValue(ref _advancedSearchResultSorting, value);
    }

    public bool ArcadeSystemAsPlatform
    {
        get => _arcadeSystemAsPlatform;
        set => SetValue(ref _arcadeSystemAsPlatform, value);
    }

    public DateToUse DateToUse
    {
        get => _dateToUse;
        set => SetValue(ref _dateToUse, value);
    }

    public bool DescriptionOverviewOnly
    {
        get => _descriptionOverviewOnly;
        set => SetValue(ref _descriptionOverviewOnly, value);
    }

    public RatingToUse RatingToUse
    {
        get => _ratingToUse;
        set => SetValue(ref _ratingToUse, value);
    }

    public bool RemoveDescriptionLinks
    {
        get => _removeDescriptionLinks;
        set => SetValue(ref _removeDescriptionLinks, value);
    }

    public ObservableCollection<string> SectionsToRemove
    {
        get => _sectionsToRemove;
        set => SetValue(ref _sectionsToRemove, value);
    }

    public ObservableCollection<TagSetting> TagSettings
    {
        get => _tagSettings;
        set => SetValue(ref _tagSettings, value);
    }

    public int MaxDegreeOfParallelism { get; set; } = GetDefaultMaxDegreeOfParallelism();
    public bool ShowTopPanelButton { get; set; } = true;

    private static int GetDefaultMaxDegreeOfParallelism()
    {
        var processorCount = Environment.ProcessorCount;
        var parallelism = (int)Math.Round(processorCount * .75D, MidpointRounding.AwayFromZero);

        if (parallelism == processorCount)
            parallelism--;

        if (parallelism < 1)
            parallelism = 1;

        return parallelism;
    }

    public void PopulateTagSettings()
    {
        if (_tagSettings is null)
        {
            _tagSettings =
            [
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
                }
            ];
        }
    }
}