using AngleSharp.Dom;
using KNARZhelper;
using Playnite.SDK.Plugins;
using System.Collections.Generic;
using UVLMetadata.Models;

namespace UVLMetadata.Parser;

public class GalleryParser
{
    public List<UVLImageFileOption> Parse(IDocument galleryData)
    {
        var sections = galleryData.QuerySelectorAll("main .container-fluid .w-100");
        var results = new List<UVLImageFileOption>();

        if (sections is null || sections.Length == 0)
        {
            return results;
        }

        foreach (var section in sections)
        {
            var header = section.QuerySelector("h2");
            var images = section.QuerySelectorAll(".galleryitem > a");

            if (header is null || images is null || images.Length == 0)
            {
                continue;
            }

            var metadataField =
                header?.TextContent.Trim() == "Box-art / Flyer"
                    ? MetadataField.CoverImage
                    : MetadataField.BackgroundImage;

            foreach (var image in images)
            {
                var imageUrl = image.GetAttribute("href") ?? string.Empty;

                if (imageUrl.IsNullOrEmpty())
                {
                    continue;
                }

                var imageOption = new UVLImageFileOption
                {
                    ImageType = metadataField,
                    Path = imageUrl
                };

                results.Add(imageOption);
            }
        }

        return results;
    }
}
