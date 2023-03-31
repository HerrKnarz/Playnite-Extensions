using Playnite.SDK.Models;
using System.Collections.Generic;

namespace WikipediaMetadata.Models
{
    /// <summary>
    /// Class with all relevant metadata fields
    /// </summary>
    internal class WikipediaGameMetadata
    {
        /// <summary>
        /// Name of the game
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Unique key - is the variable part of the page url.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        public string CoverImageUrl { get; set; }
        public ReleaseDate? ReleaseDate { get; set; }
        public List<MetadataProperty> Genres { get; set; }
        public List<MetadataProperty> Developers { get; set; }
        public List<MetadataProperty> Publishers { get; set; }
        public List<MetadataProperty> Features { get; set; }
        public List<MetadataProperty> Tags { get; set; }
        public List<Link> Links { get; set; }
        public List<MetadataProperty> Series { get; set; }
        public List<MetadataProperty> Platforms { get; set; }
        public int CriticScore { get; set; } = -1;
    }
}