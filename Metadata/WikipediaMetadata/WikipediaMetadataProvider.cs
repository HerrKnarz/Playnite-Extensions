using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    public class WikipediaMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions options;
        private readonly WikipediaMetadata plugin;

        private WikipediaGameMetadata foundGame;

        private WikipediaHtmlParser htmlParser;

        public override List<MetadataField> AvailableFields => plugin.SupportedFields;

        public WikipediaMetadataProvider(MetadataRequestOptions options, WikipediaMetadata plugin)
        {
            this.options = options;
            this.plugin = plugin;
        }

        /// <summary>
        /// Gets the page to the game from Wikipedia
        /// </summary>
        /// <returns></returns>
        private WikipediaGameMetadata FindGame()
        {
            // If we already found the game, we simply return it.
            if (foundGame != null)
            {
                return foundGame;
            }

            WikipediaGameData page = new WikipediaGameData();
            string pageHtml = string.Empty;

            try
            {
                string key = string.Empty;
                string wikiNameVideoGame = (options.GameData.Name + " (video game)").RemoveSpecialChars().ToLower().Replace(" ", "");
                string wikiName = options.GameData.Name.RemoveSpecialChars().ToLower().Replace(" ", "");
                string wikiStart = wikiName.Substring(0, (wikiName.Length > 5) ? 5 : wikiName.Length);


                if (options.IsBackgroundDownload)
                {
                    // We search for the game name on Wikipedia
                    WikipediaSearchResult searchResult = WikipediaApiCaller.GetSearchResults(options.GameData.Name);
                    if (searchResult.Pages != null && searchResult.Pages.Count > 0)
                    {
                        // Since name games have names, that aren't exclusive to video games, often "(video game)" is added to the
                        // page title, so we try that first, before searching the name itself. Only if we get a 100% match, we'll
                        // use the page in background mode.
                        Page foundPage = searchResult.Pages.Where(p => p.KeyMatch == wikiNameVideoGame).FirstOrDefault() ??
                            searchResult.Pages.Where(p => p.KeyMatch == options.GameData.Name.RemoveSpecialChars().ToLower().Replace(" ", "")).FirstOrDefault();

                        if (foundPage != null)
                        {
                            key = foundPage.Key;
                        }
                    }
                }
                else
                {
                    GenericItemOption chosen = plugin.PlayniteApi.Dialogs.ChooseItemWithSearch(null, s =>
                    {
                        // We search for the game name on Wikipedia
                        WikipediaSearchResult searchResult = WikipediaApiCaller.GetSearchResults(s);

                        List<GenericItemOption> searchResults;

                        if (plugin.Settings.Settings.AdvancedSearchResultSorting)
                        {
                            // When displaying the search results, we order them differently to hopefully get the actual game as one
                            // of the first results. First we order by containing "video game" in the short description, then by
                            // titles starting with the game name, then by titles starting with the first five characters of the game
                            // name and at last by page title itself.
                            searchResults = searchResult.Pages.Select(WikipediaItemOption.FromWikipediaSearchResult)
                                    .OrderByDescending(o => o.Description != null && o.Description.Contains("video game"))
                                    .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").StartsWith(wikiNameVideoGame))
                                    .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").StartsWith(wikiStart))
                                    .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").Contains(wikiName))
                                    .ToList<GenericItemOption>();
                        }
                        else
                        {
                            searchResults = searchResult.Pages.Select(WikipediaItemOption.FromWikipediaSearchResult).ToList<GenericItemOption>();
                        }
                        return searchResults;
                    }, options.GameData.Name, $"{plugin.Name}: {ResourceProvider.GetString("LOCWikipediaMetadataSearchDialog")}");


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
                Log.Error(ex, $"Error loading data from Wikipedia");
            }

            return foundGame = new WikipediaGameMetadata(page, plugin);
        }

        /// <summary>
        /// Similar to searching the game we only parse the html when needed (by requesting the description or links metadata)
        /// </summary>
        /// <param name="key">Page key to fetch the html</param>
        /// <returns>Parsed result with the description and additional links</returns>
        private WikipediaHtmlParser ParseHtml(string key)
        {
            if (htmlParser != null)
            {
                return htmlParser;
            }
            else
            {
                return htmlParser = new WikipediaHtmlParser(key, plugin);
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
        public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args)
        {
            return FindGame().ReleaseDate ?? base.GetReleaseDate(args);
        }
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