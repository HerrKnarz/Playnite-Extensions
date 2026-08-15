using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System.Collections.Generic;

namespace UVLMetadata.Models
{
    public enum TagCategoryId
    {
        Activity,
        Cartoon,
        Culture,
        Creature,
        Display,
        FictionGenre,
        GameEngine,
        GameGenre,
        Hardware,
        Historical,
        Location,
        Music,
        Other,
        Perspective,
        PlayerOptions,
        Software,
        Sport,
        Tool,
        Traditional,
        Vehicle,
        VideoGame,
        Advisories,
    }

    public class TagCategories : Dictionary<TagCategoryId, TagCategory>
    {
        public TagCategories()
        {
            Add(TagCategoryId.Activity,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryActivity",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/18-Activities"
                });

            Add(TagCategoryId.Cartoon,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryCartoon",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/3-Cartoons"
                });

            Add(TagCategoryId.Culture,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryCulture",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/2-Culture"
                });

            Add(TagCategoryId.Creature,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryCreature",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/13-Creatures"
                });

            Add(TagCategoryId.Display,
                new TagCategory()
                {
                    Caption = "display:",
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryDisplay",
                });

            Add(TagCategoryId.FictionGenre,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryFictionGenre",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/20-Fiction genre"
                });

            Add(TagCategoryId.GameEngine,
                new TagCategory()
                {
                    Caption = "game engine:",
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryGameEngine",
                });

            Add(TagCategoryId.GameGenre,
                new TagCategory()
                {
                    ImportAs = MetadataField.Genres,
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryGameGenre",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/19-Game genres"
                });

            Add(TagCategoryId.Hardware,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryHardware",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/10-Hardware"
                });

            Add(TagCategoryId.Historical,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryHistorical",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/4-Historical"
                });

            Add(TagCategoryId.Location,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryLocation",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/15-Locations"
                });

            Add(TagCategoryId.Music,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryMusic",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/12-Music"
                });

            Add(TagCategoryId.Other,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryOther",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/9-Other"
                });

            Add(TagCategoryId.Perspective,
                new TagCategory()
                {
                    Caption = "perspective:",
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryPerspective",
                });

            Add(TagCategoryId.PlayerOptions,
                new TagCategory()
                {
                    Caption = "player options:",
                    ImportAs = MetadataField.Features,
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryPlayerOptions",
                });

            Add(TagCategoryId.Software,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategorySoftware",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/11-Software"
                });

            Add(TagCategoryId.Sport,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategorySport",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/7-Sport"
                });

            Add(TagCategoryId.Tool,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryTool",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/16-Tools"
                });

            Add(TagCategoryId.Traditional
                ,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryTraditional",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/17-Traditional"
                });

            Add(TagCategoryId.Vehicle,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryVehicle",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/14-Vehicles"
                });

            Add(TagCategoryId.VideoGame,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryVideoGame",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/1-Video game"
                });

            Add(TagCategoryId.Advisories,
                new TagCategory()
                {
                    TranslationResourceKey = "LOCUVLMetadataTagCategoryAdvisories",
                    Url = $"{Resources.WebsiteUrl}/groups/browse/21-Advisories"
                });

            this[TagCategoryId.GameGenre].Prefix = string.Empty;
            this[TagCategoryId.PlayerOptions].Prefix = string.Empty;
        }
    }

    public class TagCategory
    {
        [DontSerialize]
        public string Caption { get; set; }

        public bool Enabled { get; set; } = true;

        public MetadataField ImportAs { get; set; } = MetadataField.Tags;

        [DontSerialize]
        public string Name { get; set; }

        public string Prefix { get; set; }

        [DontSerialize]
        public string TranslationResourceKey
        {
            get;
            set
            {
                field = value;
                Name = ResourceProvider.GetString(value);
                Prefix ??= $"[{Name}] ";
            }
        }

        [DontSerialize]
        public string Url { get; set; }
    }
}
