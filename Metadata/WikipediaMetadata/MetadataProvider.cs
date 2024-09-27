using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    public class MetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions _options;
        private readonly WikipediaMetadata _plugin;
        private WikipediaGameMetadata _foundGame;
        private HtmlParser _htmlParser;

        public MetadataProvider(MetadataRequestOptions options, WikipediaMetadata plugin)
        {
            _options = options;
            _plugin = plugin;
        }

        public override List<MetadataField> AvailableFields => _plugin.SupportedFields;

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
            var description = ParseHtml(FindGame().Key).Description;
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
            var tags = FindGame().Tags;
            return tags?.Any() ?? false ? tags : base.GetTags(args);
        }

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

            var page = new WikipediaPage();

            try
            {
                var gameFinder = new GameFinder(_plugin.Settings.Settings.AdvancedSearchResultSorting);
                var key = string.Empty;

                if (_options.IsBackgroundDownload)
                {
                    var foundPage = gameFinder.FindGame(_options.GameData.Name);

                    if (foundPage != null)
                    {
                        key = foundPage.Key;
                    }
                }
                else
                {
                    var chosen = _plugin.PlayniteApi.Dialogs.ChooseItemWithSearch(null, gameFinder.GetSearchResults, _options.GameData.Name, $"{_plugin.Name}: {ResourceProvider.GetString("LOCWikipediaMetadataSearchDialog")}");

                    if (chosen != null)
                    {
                        key = ((WikipediaItemOption)chosen).Key;
                    }
                }

                if (key != string.Empty)
                {
                    page = WikipediaApiCaller.GetGameData(key);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading data from Wikipedia");
            }

            var wikitextParser = new WikitextParser(_plugin.Settings.Settings);

            wikitextParser.Parse(page, _plugin.PlayniteApi.Database.Platforms);

            return _foundGame = wikitextParser.GameMetadata;
        }

        /// <summary>
        /// Similar to searching the game we only parse the html when needed (by requesting the description or links metadata)
        /// </summary>
        /// <param name="key">Page key to fetch the html</param>
        /// <returns>Parsed result with the description and additional links</returns>
        private HtmlParser ParseHtml(string key) => _htmlParser ?? (_htmlParser = new HtmlParser(key, _plugin.Settings.Settings));
    }
}