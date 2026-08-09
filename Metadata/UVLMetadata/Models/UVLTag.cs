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

        public string Description { get; set; }

        public int GameCount { get; set; } = 0;

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

        public TagType Type { get; set; }
    }
}
