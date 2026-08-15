using Playnite.SDK.Models;
using System.Collections.Generic;

namespace UVLMetadata.Models;

/// <summary>
/// Class with all relevant metadata fields
/// </summary>
public class UVLGameMetadata
{
    public List<MetadataProperty> AgeRatings { get; set; }
    public int CriticScore { get; set; } = -1;
    public string Description { get; set; }
    public List<MetadataProperty> Developers { get; set; }
    public List<MetadataProperty> Features { get; set; }
    public List<UVLImageFileOption> FoundImages { get; set; }
    public string GalleryUrl { get; set; } = string.Empty;
    public List<MetadataProperty> Genres { get; set; }
    public List<Link> Links { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<MetadataProperty> Platforms { get; set; }
    public List<MetadataProperty> Publishers { get; set; }
    public ReleaseDate? ReleaseDate { get; set; }
    public List<MetadataProperty> Series { get; set; }
    public List<MetadataProperty> Tags { get; set; }
    public string Url { get; set; } = string.Empty;
}
