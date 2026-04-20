using CommunityToolkit.Mvvm.ComponentModel;
using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.UICommon;
using PlayniteExtensionHelpers.WebCommon;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;

namespace LinkUtilities.DataHandlers;

internal partial class LinkPropertyGameFiltererSettings : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<SelectableObject<WebLinkType>> LinkTypes { get; set; } = [];

    [ObservableProperty]
    public partial string? SelectedText { get; set; }
}

internal class LinkPropertyGameFilterer : GameFilterer
{
    private readonly LinkUtilitiesPlugin plugin;

    public LinkPropertyGameFilterer(LinkUtilitiesPlugin plugin, Plugin.GetGameFilterersArgs args) : base(args)
    {
        this.plugin = plugin;

        if (LinkUtilitiesPlugin.PlayniteApi is null)
        {
            return;
        }

        Settings.LinkTypes.Clear();

        Settings.LinkTypes.AddMissing(LinkUtilitiesPlugin.PlayniteApi.Library.WebLinkTypes.OrderBy(l => l.Name).Select(l => new SelectableObject<WebLinkType>(l)));

        if (args.Settings is not null && !args.Settings.SerializedSettings.IsNullOrEmpty())
        {
            var selectedLinkTypes = JsonSerializer.Deserialize<HashSet<string>>(args.Settings.SerializedSettings);

            if (selectedLinkTypes.HasItems())
            {
                Settings.LinkTypes.ForEach(l => l.IsSelected = selectedLinkTypes.Any(t => t == l.ObjectData.Id));
            }

            Settings.SelectedText = GetSelectedText();
        }

        Settings.LinkTypes.ForEach(l => l.PropertyChanged += SettingsOnPropertyChanged);

        View = new LinkPropertyFilterView
        {
            DataContext = this
        };

        IsActive = Settings.LinkTypes.Any(l => l.IsSelected);
    }

    public LinkPropertyGameFiltererSettings Settings { get; } = new();

    public override void ApplyExplorerFilter(ApplyExplorerFilterArgs args)
    {
        if (args.FilterData.Data is string stringVal)
        {
            Settings.LinkTypes.ForEach(l => l.IsSelected = l.ObjectData.Id.Equals(stringVal));
        }
    }

    public override void BeginFiltering(BeginFilteringArgs args)
    {
    }

    public override void ClearFilter(ClearFilterArgs args) => Settings.LinkTypes.ForEach(l => l.IsSelected = false);

    public override void EndFiltering(EndFilteringArgs args)
    {
    }

    public override bool Filter(FilterGameArgs args)
        => args.Game.Links.HasItems() && args.Game.Links.Any(l => Settings.LinkTypes.Any(t => t.IsSelected && t.ObjectData.Id == l.TypeId));

    // This will get called when Playnite is about to save filter's settings. Usually when a user is
    // saving specific view configuration or by Playnite when last used filtering is saved.
    public override SerializeSettingsResult? SerializeSettings(SerializeSettingsArgs args)
    {
        var selectedLinkTypes = Settings.LinkTypes.Where(l => l.IsSelected).Select(l => l.ObjectData.Id).ToHashSet();

        return new SerializeSettingsResult(JsonSerializer.Serialize(selectedLinkTypes, WebHelper.DefaultJsonSerializerOptions));
    }

    private string? GetSelectedText() => Settings.LinkTypes.HasItems() ? string.Join(", ", Settings.LinkTypes.Where(l => l.IsSelected).Select(l => l.ObjectData.Name)) : null;

    private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        IsActive = Settings.LinkTypes.Any(l => l.IsSelected);

        Settings.SelectedText = GetSelectedText();

        if (IsActive)
        {
            FilterChangedAsync(new FilterChangedArgs());
        }
    }
}