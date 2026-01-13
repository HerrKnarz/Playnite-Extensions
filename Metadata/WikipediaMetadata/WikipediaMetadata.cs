using Playnite.SDK;
using Playnite.SDK.Plugins;
using PlayniteExtensions.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using WikipediaCategories;
using WikipediaCategories.BulkImport;

namespace WikipediaMetadata;

public class WikipediaMetadata : MetadataPlugin
{
    public WikipediaMetadataSettingsViewModel Settings { get; set; }

    public override Guid Id { get; } = Guid.Parse("6c1bdd62-77bf-4866-a264-11544508687c");

    public override List<MetadataField> SupportedFields { get; } =
    [
        MetadataField.Name,
        MetadataField.ReleaseDate,
        MetadataField.Genres,
        MetadataField.Developers,
        MetadataField.Publishers,
        MetadataField.Features,
        MetadataField.Tags,
        MetadataField.Links,
        MetadataField.Series,
        MetadataField.Platform,
        MetadataField.CoverImage,
        MetadataField.CriticScore,
        MetadataField.Description
    ];

    public override string Name => "Wikipedia";

    private readonly WikipediaApi _api;

    public WikipediaMetadata(IPlayniteAPI api) : base(api)
    {
        Settings = new WikipediaMetadataSettingsViewModel(this);
        Properties = new MetadataPluginProperties
        {
            HasSettings = true
        };
        _api = new(new WebDownloader(), PlayniteApi.ApplicationInfo.ApplicationVersion);
    }

    public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options) => new MetadataProvider(options, this, _api);

    public override ISettings GetSettings(bool firstRunSettings) => Settings;

    public override UserControl GetSettingsView(bool firstRunSettings) => new WikipediaMetadataSettingsView();
    
    public override IEnumerable<TopPanelItem> GetTopPanelItems()
    {
        if (!Settings.Settings.ShowTopPanelButton)
            yield break;

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var iconPath = Path.Combine(Path.GetDirectoryName(assemblyLocation)!, "icon.png");
        yield return new()
        {
            Icon = iconPath,
            Visible = true,
            Title = "Import Wikipedia category",
            Activated = ImportGameProperty
        };
    }

    public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
    {
        if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            yield return new() { MenuSection = "@Wikipedia", Description = "Import Wikipedia category", Action = _ => ImportGameProperty(), };
    }

    private void ImportGameProperty()
    {
        var api = new WikipediaApi(new WebDownloader(), PlayniteApi.ApplicationInfo.ApplicationVersion);
        var searchProvider = new WikipediaCategorySearchProvider(api);
        var bulk = new WikipediaCategoryBulkImport(PlayniteApi.Database, new(PlayniteApi), searchProvider, new PlatformUtility(PlayniteApi), Settings.Settings.MaxDegreeOfParallelism);
        bulk.ImportGameProperty();
    }
}