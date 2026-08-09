using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace UVLMetadata.Models
{
    public class UVLImageFileOption : ImageFileOption
    {
        public MetadataField ImageType { get; set; } = MetadataField.BackgroundImage;
    }
}
