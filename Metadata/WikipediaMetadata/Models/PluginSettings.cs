using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WikipediaMetadata.Models;

public class PluginSettings : ObservableObject
{
    private ObservableCollection<TagSetting> _tagSettings;

    public bool AdvancedSearchResultSorting { get; set => SetValue(ref field, value); } = true;

    public bool ArcadeSystemAsPlatform { get; set => SetValue(ref field, value); }

    public DateToUse DateToUse { get; set => SetValue(ref field, value); } = DateToUse.Earliest;

    public bool DescriptionOverviewOnly { get; set => SetValue(ref field, value); }

    public RatingToUse RatingToUse { get; set => SetValue(ref field, value); } = RatingToUse.Average;

    public bool RemoveDescriptionLinks { get; set => SetValue(ref field, value); }

    public ObservableCollection<string> SectionsToRemove { get; set => SetValue(ref field, value); } = [];

    public ObservableCollection<TagSetting> TagSettings { get => _tagSettings; set => SetValue(ref _tagSettings, value); }

    public int MaxDegreeOfParallelism { get; set; } = GetDefaultMaxDegreeOfParallelism();
    public bool ShowTopPanelButton { get; set; } = true;
    public bool ImportCategories { get; set; } = false;

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
            _tagSettings = [];
        else
            _tagSettings = DistinctByName(_tagSettings).ToObservable(); // Hotfix to a bug that duplicated the tag settings in version 1.3 and 1.4

        AddMissingTagSetting("Arcade System", "[Arcade System]");
        AddMissingTagSetting("Engine", "[Game Engine]");
        AddMissingTagSetting("Categories", "[Category]");
        AddMissingTagSetting("Director", "[People] director:");
        AddMissingTagSetting("Producer", "[People] producer:");
        AddMissingTagSetting("Designer", "[People] designer:");
        AddMissingTagSetting("Programmer", "[People] programmer:");
        AddMissingTagSetting("Artist", "[People] artist:");
        AddMissingTagSetting("Writer", "[People] writer:");
        AddMissingTagSetting("Composer", "[People] composer:");
    }

    private void AddMissingTagSetting(string name, string prefix)
    {
        if (_tagSettings.Any(ts => ts.Name == name))
            return;

        _tagSettings.Add(new()
        {
            IsChecked = true,
            Name = name,
            Prefix = prefix,
        });
    }

    private static IEnumerable<TagSetting> DistinctByName(IEnumerable<TagSetting> tagSettings)
    {
        HashSet<string> tagSettingNames = [];
        foreach (var tagSetting in tagSettings)
        {
            if (tagSettingNames.Add(tagSetting.Name))
                yield return tagSetting;
        }
    }
}
