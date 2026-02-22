using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using WikipediaMetadata.Models;

namespace WikipediaMetadata;

public class MetadataProvider(MetadataRequestOptions options, PluginSettings settings, IPlayniteAPI playniteApi, WikipediaApi api) : OnDemandMetadataProvider
{
    private WikipediaGameMetadata _foundGame;
    private HtmlParser _htmlParser;

    public override List<MetadataField> AvailableFields => WikipediaMetadata.Fields;

    public override MetadataFile GetCoverImage(GetMetadataFieldArgs args)
    {
        var coverImageUrl = FindGame().CoverImageUrl;
        return string.IsNullOrEmpty(coverImageUrl) ? base.GetCoverImage(args) : new MetadataFile(coverImageUrl);
    }

    public override int? GetCriticScore(GetMetadataFieldArgs args)
    {
        var criticScore = FindGame().CriticScore;
        return criticScore > -1 ? criticScore : base.GetCriticScore(args);
    }

    public override string GetDescription(GetMetadataFieldArgs args)
    {
        var key = FindGame().Key;

        if (key == string.Empty)
        {
            return base.GetDescription(args);
        }

        var description = ParseHtml(key).Description;
        return string.IsNullOrEmpty(description) ? base.GetDescription(args) : description;
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

        links?.AddMissing(ParseHtml(FindGame().Key).Links);

        return links?.Any() ?? false ? links : base.GetLinks(args);
    }

    public override string GetName(GetMetadataFieldArgs args)
    {
        var name = FindGame().Name;
        return string.IsNullOrEmpty(name) ? base.GetName(args) : name;
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
        var foundGame = FindGame();

        var categorySettings = settings.TagSettings.FirstOrDefault(ts => ts.Name == "Categories");
        if (categorySettings?.IsChecked != true || foundGame.Categories == null)
        {
            return foundGame.Tags;
        }

        var tags = foundGame.Tags ?? [];

        HashSet<string> excludedCategoryStarts =
        [
            "Articles ", "All articles ", "All Wikipedia articles ", "CS1", "Use ", ..foundGame.InfoBoxLinkedArticles,
            ..foundGame.Developers.OfType<MetadataNameProperty>().Select(d => d.Name),
            ..foundGame.Publishers.OfType<MetadataNameProperty>().Select(d => d.Name),
        ];
        var excludedCategories = foundGame.InfoBoxLinkedArticles.Select(GetCategoryNameFromArticle).Where(x => x != null).ToHashSet();
        foreach (string category in foundGame.Categories)
        {
            string strippedName = category.StripCategoryPrefix();
            bool skipCategory = excludedCategoryStarts.Any(a => strippedName.StartsWith(a, StringComparison.InvariantCultureIgnoreCase))
                                || strippedName.Contains("Wikidata")
                                || excludedCategories.Contains(strippedName);

            if (skipCategory)
            {
                continue;
            }

            string nameWithPrefix = $"{categorySettings.Prefix} {strippedName}".Trim();

            tags.Add(new MetadataNameProperty(nameWithPrefix));
        }

        return tags.Any() ? tags : null; //return null to allow Playnite to skip to other metadata providers' results
    }

    /// <summary>
    /// Some articles aren't the first part of their category names - get those articles' exact
    /// corresponding category names to exclude
    /// </summary>
    /// <param name="articleName"></param>
    /// <returns></returns>
    private static string GetCategoryNameFromArticle(string articleName) => articleName switch
    {
        "Xbox (console)" => "Xbox games",
        "Mac OS X" => "MacOS games",
        "Microsoft Windows" => "Windows games",
        "Nintendo Wii" => "Wii games",
        "Platform game" => "Platformers",
        "Xbox Series X/S" => "Xbox Series X and Series S games",
        _ => null,
    };

    /// <summary>
    /// Gets the page to the game from Wikipedia
    /// </summary>
    /// <returns></returns>
    private WikipediaGameMetadata FindGame()
    {
        // If we already found the game, we simply return it.
        if (_foundGame != null)
        {
            return _foundGame;
        }

        _foundGame = new WikipediaGameMetadata();

        try
        {
            var gameFinder = new GameFinder(settings.AdvancedSearchResultSorting);

            string key;

            if (options.IsBackgroundDownload)
            {
                key = gameFinder.FindGame(options.GameData.Name)?.Key;
            }
            else
            {
                var chosen = playniteApi.Dialogs.ChooseItemWithSearch(null, gameFinder.GetSearchResults,
                                                                      options.GameData.Name,
                                                                      $"Wikipedia: {ResourceProvider.GetString("LOCWikipediaMetadataSearchDialog")}");

                if (chosen is not WikipediaItemOption option)
                {
                    return _foundGame;
                }

                key = option.Key;
            }

            if (key != string.Empty)
            {
                var wikitextParser = new WikitextParser(settings, api);

                wikitextParser.Parse(api.GetGameData(key), playniteApi.Database.Platforms);

                return _foundGame = wikitextParser.GameMetadata;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading data from Wikipedia");
            throw;
        }

        return _foundGame;
    }

    /// <summary>
    /// Similar to searching the game we only parse the HTML when needed (by requesting the
    /// description or links metadata)
    /// </summary>
    /// <param name="key">Page key to fetch the HTML</param>
    /// <returns>Parsed result with the description and additional links</returns>
    private HtmlParser ParseHtml(string key) => _htmlParser ??= new(key, settings, api);
}
