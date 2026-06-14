using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkUtilities.Models;
using LinkUtilities.Views;
using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.FilesCommon;
using PlayniteExtensionHelpers.WebCommon;
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

    public static List<WebLinkType> LinkTypes => LinkUtilitiesPlugin.PlayniteApi?.Library.WebLinkTypes?.OrderBy(t => t.Name, StringComparer.CurrentCultureIgnoreCase).ToList() ?? [];

    [ObservableProperty]
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
            settings = JsonSerializer.Deserialize<LinkUtilitiesPluginSettings>(json, WebHelper.DefaultJsonSerializerOptions);
        }

        settings ??= new LinkUtilitiesPluginSettings();

        if (LinkUtilitiesPlugin.Plugin is not null)
        {
            settings.LinkSettings.RefreshLinkSources(LinkUtilitiesPlugin.Plugin.Links);
        }

        settings.LinkSettings = new LinkSourceSettings([.. settings.LinkSettings.OrderBy(x => x.LinkName, StringComparer.CurrentCultureIgnoreCase)]);

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
    private static void PatternHelpClick() => Process.Start(new ProcessStartInfo("https://knarzwerk.de/en/playnite-extensions/link-utilities/url-handler-and-bookmarklet/") { UseShellExecute = true });

    [RelayCommand]
    private static void WebsiteHelpClick() => Process.Start(new ProcessStartInfo("https://knarzwerk.de/en/playnite-extensions/link-utilities/supported-websites-for-add-search-function/") { UseShellExecute = true });

    [RelayCommand]
    private void AddDefaultLinkNamePatterns() => Settings.LinkNamePatterns.AddDefaultPatterns(PatternTypes.LinkNamePattern);

    [RelayCommand]
    private void AddLinkNamePattern()
    {
        var pattern = new LinkNamePattern();

        Settings.LinkNamePatterns.Add(pattern);

        SelectedPattern = pattern;
    }

    [RelayCommand]
    private void RemoveLinkNamePatterns(object item)
    {
        if (item is not LinkNamePattern linkPattern)
        {
            return;
        }

        Settings.LinkNamePatterns.Remove(linkPattern);
    }

    [RelayCommand]
    private void SortPatterns() => Settings.LinkNamePatterns.SortPatterns();
}