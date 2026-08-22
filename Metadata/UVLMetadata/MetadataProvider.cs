using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using UVLMetadata.Models;
using UVLMetadata.Parser;

namespace UVLMetadata;

public class MetadataProvider(MetadataRequestOptions options, UVLMetadata plugin) : OnDemandMetadataProvider
{
    private UVLGameMetadata _foundGame;

    public override List<MetadataField> AvailableFields => UVLMetadata.Fields;

    public override IEnumerable<MetadataProperty> GetAgeRatings(GetMetadataFieldArgs args)
    {
        var ageRatings = FindGame().AgeRatings;
        return ageRatings?.Any() ?? false ? ageRatings : base.GetAgeRatings(args);
    }

    //public override MetadataFile GetBackgroundImage(GetMetadataFieldArgs args) => GetImage(MetadataField.BackgroundImage, args);
    //public override MetadataFile GetCoverImage(GetMetadataFieldArgs args) => GetImage(MetadataField.CoverImage, args);

    public override int? GetCriticScore(GetMetadataFieldArgs args)
    {
        var criticScore = FindGame().CriticScore;
        return criticScore > -1 ? criticScore : base.GetCriticScore(args);
    }

    public override string GetDescription(GetMetadataFieldArgs args)
    {
        var description = FindGame().Description;
        return description.IsNullOrEmpty() ? base.GetDescription(args) : description;
    }

    public override IEnumerable<MetadataProperty> GetDevelopers(GetMetadataFieldArgs args)
    {
        var developers = FindGame().Developers;
        return developers?.Any() ?? false ? developers : base.GetDevelopers(args);
    }

    public override IEnumerable<MetadataProperty> GetFeatures(GetMetadataFieldArgs args)
    {
        var features = FindGame().Features;
        return features?.Any() ?? false ? features : base.GetFeatures(args);
    }

    public override IEnumerable<MetadataProperty> GetGenres(GetMetadataFieldArgs args)
    {
        var genres = FindGame().Genres;
        return genres?.Any() ?? false ? genres : base.GetGenres(args);
    }

    public override IEnumerable<Link> GetLinks(GetMetadataFieldArgs args)
    {
        var links = FindGame().Links;
        return links?.Any() ?? false ? links : base.GetLinks(args);
    }

    public override string GetName(GetMetadataFieldArgs args)
    {
        var name = FindGame().Name;
        return name.IsNullOrEmpty() ? base.GetName(args) : name;
    }

    public override IEnumerable<MetadataProperty> GetPlatforms(GetMetadataFieldArgs args)
    {
        var platforms = FindGame().Platforms;
        return platforms?.Any() ?? false ? platforms : base.GetPlatforms(args);
    }

    public override IEnumerable<MetadataProperty> GetPublishers(GetMetadataFieldArgs args)
    {
        var publishers = FindGame().Publishers;
        return publishers?.Any() ?? false ? publishers : base.GetPublishers(args);
    }

    public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args) => FindGame().ReleaseDate ?? base.GetReleaseDate(args);

    public override IEnumerable<MetadataProperty> GetSeries(GetMetadataFieldArgs args)
    {
        var series = FindGame().Series;
        return series?.Any() ?? false ? series : base.GetSeries(args);
    }

    public override IEnumerable<MetadataProperty> GetTags(GetMetadataFieldArgs args)
    {
        var tags = FindGame().Tags;
        return tags?.Any() ?? false ? tags : base.GetTags(args);
    }

    /// <summary>
    /// Gets the page to the game from UVL
    /// </summary>
    /// <returns></returns>
    private UVLGameMetadata FindGame()
    {
        // If we already found the game, we simply return it.
        if (_foundGame is not null)
        {
            return _foundGame;
        }

        _foundGame = new UVLGameMetadata();
        plugin.UVLConnect.searchedDocument = null;

        try
        {
            if (plugin.Settings.Settings.LastTagRefresh == DateTime.MinValue && API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCUVLMetadataDialogRefreshTags"), "UVL", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                plugin.UVLConnect.RefreshTags();
                plugin.Settings.Settings.LastTagRefresh = DateTime.Now;
            }

            string url;

            if (options.IsBackgroundDownload)
            {
                url = plugin.UVLConnect.FindGame(options.GameData)?.Url;
            }
            else
            {
                var chosen = API.Instance.Dialogs.ChooseItemWithSearch(null,
                    plugin.UVLConnect.GetSearchResults,
                    options.GameData.Name,
                    $"UVL: {ResourceProvider.GetString("LOCUVLMetadataSearchDialog")}");

                if (chosen is not UVLItemOption option)
                {
                    return _foundGame;
                }

                url = option.Url;
            }

            if (!url.IsNullOrEmpty())
            {
                var uvlGamePageParser = new GamePageParser(plugin.Settings.Settings, plugin.UVLConnect);

                if (plugin.UVLConnect.searchedDocument is null)
                {
                    uvlGamePageParser.Parse(url, plugin.UVLConnect.GetGameData(url));
                }
                else
                {
                    uvlGamePageParser.Parse(url, plugin.UVLConnect.searchedDocument);
                }

                return _foundGame = uvlGamePageParser.GameMetadata;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading data from UVL");
            throw;
        }

        return _foundGame;
    }

    private MetadataFile GetImage(MetadataField imageType, GetMetadataFieldArgs args)
    {
        FindGame();
        GetImages();

        if (options.IsBackgroundDownload)
        {
            var gameUrl = _foundGame?.FoundImages.FirstOrDefault(p => p.ImageType == imageType)?.Path;

            return gameUrl.IsNullOrEmpty() ? base.GetCoverImage(args) : new MetadataFile(gameUrl);
        }

        var selection = new List<ImageFileOption>();

        selection.AddRange(_foundGame?.FoundImages.Where(p => p.ImageType == imageType) ?? []);

        if (selection.Count == 0)
        {
            return base.GetCoverImage(args);
        }

        if (selection.Count == 1)
        {
            return new MetadataFile(selection[0].Path);
        }

        var dialogCaption = imageType == MetadataField.CoverImage
            ? ResourceProvider.GetString("LOCUVLMetadataChooseCoverImage")
            : ResourceProvider.GetString("LOCUVLMetadataChooseBackgroundImage");

        var selectedImage = API.Instance.Dialogs.ChooseImageFile(
            selection, $"UVL: {dialogCaption}");

        return selectedImage is null
            ? imageType == MetadataField.CoverImage
                ? base.GetCoverImage(args)
                : base.GetBackgroundImage(args)
            : new MetadataFile(selectedImage.Path);
    }

    private void GetImages()
    {
        if (_foundGame is null)
        {
            return;
        }

        if (_foundGame.FoundImages is null)
        {
            var galleryParser = new GalleryParser();

            var galleryData = plugin.UVLConnect.LoadDocument(_foundGame.GalleryUrl);

            _foundGame.FoundImages = galleryParser.Parse(galleryData);
        }
    }
}
