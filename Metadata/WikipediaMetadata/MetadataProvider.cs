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

        public override List<MetadataField> AvailableFields => _plugin.SupportedFields;

        public MetadataProvider(MetadataRequestOptions options, WikipediaMetadata plugin)
        {
            _options = options;
            _plugin = plugin;
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

            WikipediaPage page = new WikipediaPage();
            string pageHtml = string.Empty;

            try
            {
                GameFinder gameFinder = new GameFinder(_plugin.Settings.Settings);
                string key = string.Empty;

                if (_options.IsBackgroundDownload)
                {
                    Page foundPage = gameFinder.FindGame(_options.GameData.Name);

                    if (foundPage != null)
                    {
                        key = foundPage.Key;
                    }
                }
                else
                {
                    GenericItemOption chosen = _plugin.PlayniteApi.Dialogs.ChooseItemWithSearch(null, gameFinder.GetSearchResults, _options.GameData.Name, $"{_plugin.Name}: {ResourceProvider.GetString("LOCWikipediaMetadataSearchDialog")}");

                    if (chosen != null)
                    {
                        key = ((WikipediaItemOption)chosen).Key;
                    }
                }

                if (key != string.Empty)
                {
                    page = ApiCaller.GetGameData(key);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from Wikipedia");
            }

            return _foundGame = new WikitextParser(_plugin.Settings.Settings, page, _plugin.PlayniteApi.Database.Platforms).GameMetadata;
        }

        /// <summary>
        /// Similar to searching the game we only parse the html when needed (by requesting the description or links metadata)
        /// </summary>
        /// <param name="key">Page key to fetch the html</param>
        /// <returns>Parsed result with the description and additional links</returns>
        private HtmlParser ParseHtml(string key)
        {
            if (_htmlParser != null)
            {
                return _htmlParser;
            }
            else
            {
                return _htmlParser = new HtmlParser(key, _plugin.Settings.Settings);
            }
        }

        public override MetadataFile GetCoverImage(GetMetadataFieldArgs args)
        {
            string coverImageUrl = FindGame().CoverImageUrl;
            return string.IsNullOrEmpty(coverImageUrl) ? base.GetCoverImage(args) : new MetadataFile(coverImageUrl);
        }

        public override string GetName(GetMetadataFieldArgs args)
        {
            string name = FindGame().Name;
            return string.IsNullOrEmpty(name) ? base.GetName(args) : name;
        }
        public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args) => FindGame().ReleaseDate ?? base.GetReleaseDate(args);
        public override IEnumerable<MetadataProperty> GetGenres(GetMetadataFieldArgs args)
        {
            List<MetadataProperty> genres = FindGame().Genres;
            return (genres?.Any() ?? false) ? genres : base.GetGenres(args);
        }
        public override IEnumerable<MetadataProperty> GetDevelopers(GetMetadataFieldArgs args)
        {
            List<MetadataProperty> developers = FindGame().Developers;
            return (developers?.Any() ?? false) ? developers : base.GetDevelopers(args);
        }
        public override IEnumerable<MetadataProperty> GetPublishers(GetMetadataFieldArgs args)
        {
            List<MetadataProperty> publishers = FindGame().Publishers;
            return (publishers?.Any() ?? false) ? publishers : base.GetPublishers(args);
        }
        public override IEnumerable<MetadataProperty> GetFeatures(GetMetadataFieldArgs args)
        {
            List<MetadataProperty> features = FindGame().Features;
            return (features?.Any() ?? false) ? features : base.GetFeatures(args);
        }
        public override IEnumerable<MetadataProperty> GetTags(GetMetadataFieldArgs args)
        {
            List<MetadataProperty> tags = FindGame().Tags;
            return (tags?.Any() ?? false) ? tags : base.GetTags(args);
        }
        public override IEnumerable<Link> GetLinks(GetMetadataFieldArgs args)
        {
            List<Link> links = FindGame().Links;

            links.AddMissing(ParseHtml(FindGame().Key).Links);

            return (links?.Any() ?? false) ? links : base.GetLinks(args);
        }
        public override IEnumerable<MetadataProperty> GetSeries(GetMetadataFieldArgs args)
        {
            List<MetadataProperty> series = FindGame().Series;
            return (series?.Any() ?? false) ? series : base.GetSeries(args);
        }
        public override IEnumerable<MetadataProperty> GetPlatforms(GetMetadataFieldArgs args)
        {
            List<MetadataProperty> platforms = FindGame().Platforms;
            return (platforms?.Any() ?? false) ? platforms : base.GetPlatforms(args);
        }
        public override int? GetCriticScore(GetMetadataFieldArgs args)
        {
            int criticScore = FindGame().CriticScore;
            return (criticScore > -1) ? criticScore : base.GetCriticScore(args);
        }

        public override string GetDescription(GetMetadataFieldArgs args)
        {
            string description = ParseHtml(FindGame().Key).Description;
            return string.IsNullOrEmpty(description) ? base.GetDescription(args) : description;
        }
    }
}