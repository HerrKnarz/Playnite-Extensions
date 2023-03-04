using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace WikipediaMetadata
{
    public class WikipediaMetadata : MetadataPlugin
    {
        private WikipediaMetadataSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("6c1bdd62-77bf-4866-a264-11544508687c");

        public override List<MetadataField> SupportedFields { get; } = new List<MetadataField>
        {
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
            //MetadataField.CriticScore,
            //MetadataField.CommunityScore,
            //MetadataField.AgeRating,
            MetadataField.Description,
        };

        // Change to something more appropriate
        public override string Name => "Wikipedia";

        public WikipediaMetadata(IPlayniteAPI api) : base(api)
        {
            Settings = new WikipediaMetadataSettingsViewModel(this);
            Properties = new MetadataPluginProperties
            {
                HasSettings = false
            };
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new WikipediaMetadataProvider(options, this);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new WikipediaMetadataSettingsView();
        }
    }
}