using Playnite.SDK;
using Playnite.SDK.Data;
using System;

namespace UVLMetadata.Models
{
    public enum TagType
    {
        Series,
        Theme,
        Concept,
        Entity,
    }

    public class UVLTag
    {
        public TagCategoryId Category { get; set; }

        [DontSerialize]
        public string CategoryCaption { get; set; } = string.Empty;

        public string Description { get; set; }
        public int GameCount { get; set; } = 0;

        [DontSerialize]
        public string GamesCountFormatted => $"({GameCount})";

        public string Name
        {
            get;
            set
            {
                field = value;

                var shortValue = value;

                if (shortValue.Contains("("))
                {
                    shortValue = shortValue.Remove(value.IndexOf("(", StringComparison.Ordinal));
                }

                if (shortValue.Contains("["))
                {
                    shortValue = shortValue.Remove(value.IndexOf("[", StringComparison.Ordinal));
                }

                ShortName = shortValue.Trim();
            }
        }

        [DontSerialize]
        public string ShortName { get; set; }

        public string Slug { get; set; }

        public TagType Type
        {
            get;
            set
            {
                field = value;
                TypeCaption = value switch
                {
                    TagType.Series => ResourceProvider.GetString("LOCUVLMetadataTagTypeSeries"),
                    TagType.Theme => ResourceProvider.GetString("LOCUVLMetadataTagTypeTheme"),
                    TagType.Concept => ResourceProvider.GetString("LOCUVLMetadataTagTypeConcept"),
                    TagType.Entity => ResourceProvider.GetString("LOCUVLMetadataTagTypeEntity"),
                    _ => string.Empty,
                };
            }
        }

        [DontSerialize]
        public string TypeCaption { get; set; } = string.Empty;
    }
}
