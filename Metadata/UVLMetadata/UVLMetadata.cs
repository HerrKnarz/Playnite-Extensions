using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using UVLMetadata.Models;
using UVLMetadata.ViewModels;
using UVLMetadata.Views;

namespace UVLMetadata;

public class UVLMetadata : MetadataPlugin
{
    public static readonly List<MetadataField> Fields =
    [
        MetadataField.Name,
        MetadataField.Genres,
        MetadataField.ReleaseDate,
        MetadataField.Developers,
        MetadataField.Publishers,
        MetadataField.Tags,
        MetadataField.Description,
        MetadataField.Links,
        MetadataField.CriticScore,
        //MetadataField.CoverImage,
        //MetadataField.BackgroundImage,
        MetadataField.Features,
        MetadataField.Series,
        MetadataField.Platform,
        MetadataField.AgeRating,
    ];

    //TODO: Add support for CoverImage and BackgroundImage if there is demand. The problem is that UVL requires
    //a real browser to access the images, so I'd have to download and cache them myself. Not sure it's worth it,
    //since there are far better sources for images already anyway.

    public UVLMetadata(IPlayniteAPI api) : base(api)
    {
        Tags = new UVLTags(this);
        UVLConnect = new UVLConnect(this);
        Settings = new SettingsViewModel(this);
        Properties = new MetadataPluginProperties
        {
            HasSettings = true
        };

        var iconResourcesToAdd = new Dictionary<string, string>
            {
                { "tagCategoryIcon", "\xf005" }
            };

        foreach (var iconResource in iconResourcesToAdd)
        {
            MiscHelper.AddTextIcoFontResource(iconResource.Key, iconResource.Value);
        }

        Tags.LoadFromFile();
    }

    public override Guid Id { get; } = Guid.Parse("b825766b-c151-43cd-a918-7322fdc1868f");

    public override string Name => "UVL";

    public SettingsViewModel Settings { get; set; }

    public override List<MetadataField> SupportedFields => Fields;

    public UVLTags Tags { get; }

    public UVLConnect UVLConnect { get; }

    public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
    {
        var menuSection = ResourceProvider.GetString("LOCUVLMetadataName");

        var menuItems = new List<MainMenuItem>
            {
                new() {
                    Description = ResourceProvider.GetString("LOCUVLMetadataMenuBulkImport"),
                    MenuSection = $"@{menuSection}",
                    Icon = "tagCategoryIcon",
                    Action = a => BulkImportViewModel.ShowWindow(this)
                }
            };

        return menuItems;
    }

    public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options) => new MetadataProvider(options, Settings.Settings, PlayniteApi, UVLConnect);

    public override ISettings GetSettings(bool firstRunSettings) => Settings;

    public override UserControl GetSettingsView(bool firstRunSettings) => new SettingsView();
}
