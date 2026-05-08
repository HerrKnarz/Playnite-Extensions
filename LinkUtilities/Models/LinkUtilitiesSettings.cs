using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace LinkUtilities.Models;

public partial class LinkUtilitiesPluginSettings : ObservableObject
{
    [ObservableProperty]
    public partial bool AddLinksToNewGames { get; set; } = false;

    [ObservableProperty]
    public partial bool CleanUpAfterChange { get; set; } = true;

    [ObservableProperty]
    public partial bool ConvertSteamLinksAfterChange { get; set; } = false;

    [ObservableProperty]
    public partial bool DebugMode { get; set; } = false;

    [JsonIgnore]
    public DuplicateTypesWithCaptions? DuplicateTypesWithCaptions { get; set; }

    [ObservableProperty]
    public partial LinkNamePatterns LinkNamePatterns { get; set; } = new();

    [ObservableProperty]
    public partial LinkSourceSettings LinkSettings { get; set; } = new();

    [JsonIgnore]
    public bool OnlyATest { get; set; } = false;

    [ObservableProperty]
    public partial bool RemoveDuplicatesAfterChange { get; set; } = false;

    [ObservableProperty]
    public partial DuplicateTypes RemoveDuplicatesType { get; set; } = DuplicateTypes.TypeAndUrl;
}