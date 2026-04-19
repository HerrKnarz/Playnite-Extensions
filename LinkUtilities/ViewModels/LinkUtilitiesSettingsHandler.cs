using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using LinkUtilities.Views;
using Playnite;
using PlayniteCommon;
using PlayniteCommon.FilesCommon;
using PlayniteCommon.WebCommon;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace LinkUtilities.ViewModels;

[INotifyPropertyChanged]
public partial class LinkUtilitiesSettingsHandler : PluginSettingsHandler
{
    private readonly LinkUtilitiesPlugin _plugin;

    public LinkUtilitiesSettingsHandler(LinkUtilitiesPlugin plugin)
    {
        _plugin = plugin;

        Settings.DuplicateTypesWithCaptions ??= [];
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveLinkNamePatternsCommand))]
    public partial LinkNamePattern? SelectedPattern { get; set; }

    [ObservableProperty]
    public partial LinkUtilitiesPluginSettings Settings { get; set; } = LoadSettings();

    public static LinkUtilitiesPluginSettings LoadSettings()
    {
        LinkUtilitiesPluginSettings? settings = null;

        var dataDir = LinkUtilitiesPlugin.PlayniteApi?.UserDataDir;

        if (dataDir.IsNullOrEmpty())
        {
            return new LinkUtilitiesPluginSettings();
        }

        var setFile = Path.Combine(dataDir, "settings.json");

        if (File.Exists(setFile))
        {
            using var json = File.OpenRead(setFile);
            settings = JsonSerializer.Deserialize<LinkUtilitiesPluginSettings>(json);
        }

        settings ??= new LinkUtilitiesPluginSettings();

        settings.LinkSettings.RefreshLinkSources(AddWebsiteLinks.Instance().Links);
        settings.LinkSettings.RefreshLibraryLinkSettings();
        settings.LinkSettings = new LinkSourceSettings([.. settings.LinkSettings.OrderBy(x => x.LinkName)]);

        return settings;
    }

    public static void SaveSettings(LinkUtilitiesPluginSettings settings)
    {
        var dataDir = LinkUtilitiesPlugin.PlayniteApi?.UserDataDir;

        if (dataDir.IsNullOrEmpty())
        {
            return;
        }

        var setFile = Path.Combine(dataDir, "settings.json");

        FileHelper.WriteStringToFile(setFile, JsonSerializer.Serialize(settings, WebHelper.DefaultJsonSerializerOptions));
    }

    public override async Task BeginEditAsync(BeginEditArgs args)
    {
        if (Settings.LinkNamePatterns.HasItems())
        {
            SelectedPattern = Settings.LinkNamePatterns.First();
        }

        await Task.CompletedTask;
    }

    //NEXT: Either implement this or make settings immutable. Otherwise, changes to the settings will be applied immediately and can't be canceled.
    public override async Task CancelEditAsync(CancelEditArgs args) => await Task.CompletedTask;

    public override async Task EndEditAsync(EndEditArgs args)
    {
        SaveSettings(Settings);

        LinkUtilitiesPlugin.Settings = Settings;

        await Task.CompletedTask;
    }

    public override FrameworkElement GetEditView(GetSettingsViewArgs args) => new LinkUtilitiesSettingsView { DataContext = this };

    public override async Task<ICollection<string>> VerifySettingsAsync(VerifySettingsArgs args)
    {
        await Task.CompletedTask;
        return [];
    }

    [RelayCommand]
    private static void HelpBookmarklet() =>
        Process.Start(new ProcessStartInfo("https://knarzwerk.de/en/playnite-extensions/link-utilities/url-handler-and-bookmarklet/"));

    [RelayCommand]
    private void AddDefaultLinkNamePatterns() => Settings.LinkNamePatterns.AddDefaultPatterns(PatternTypes.LinkNamePattern);

    [RelayCommand]
    private void AddLinkNamePattern() => Settings.LinkNamePatterns.Add(new LinkNamePattern());

    private bool CanRemove(IList<object> items) => SelectedPattern is not null || items.HasItems();

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void RemoveLinkNamePatterns(IList<object> items)
    {
        foreach (var linkPattern in items.ToList().Cast<LinkNamePattern>())
        {
            Settings.LinkNamePatterns.Remove(linkPattern);
        }
    }

    [RelayCommand]
    private void SortBookmarkletItems() => Settings.LinkNamePatterns.SortPatterns();
}