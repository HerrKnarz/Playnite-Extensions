using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    public class WikipediaMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions options;
        private readonly WikipediaMetadata plugin;

        private readonly string baseUrl = "https://en.wikipedia.org/w/rest.php/v1/";
        private string SearchUrl { get => baseUrl + "search/page?q={0}&limit={1}"; }
        private string PageUrl { get => baseUrl + "page/{0}"; }

        private WikipediaGameMetadata foundGame;

        public override List<MetadataField> AvailableFields => plugin.SupportedFields;

        public WikipediaMetadataProvider(MetadataRequestOptions options, WikipediaMetadata plugin)
        {
            this.options = options;
            this.plugin = plugin;
        }

        private WikipediaGameMetadata FindGame()
        {
            if (foundGame != null)
            {
                return foundGame;
            }

            WikipediaGameData page = new WikipediaGameData();
            string pageHtml = string.Empty;

            try
            {
                string apiUrl = string.Format(SearchUrl, options.GameData.Name.UrlEncode(), 50);

                WebClient client = new WebClient();

                Assembly thisAssem = typeof(WikipediaMetadata).Assembly;
                AssemblyName thisAssemName = thisAssem.GetName();

                Version version = thisAssemName.Version;

                client.Headers.Add("user-agent", $"Playnite Wikipedia Metadata Addon/{version} (alex@knarzwerk.de)");
                client.Encoding = Encoding.UTF8;

                string jsonResult = client.DownloadString(apiUrl);

                string key = string.Empty;

                WikipediaSearchResult searchResult = Serialization.FromJson<WikipediaSearchResult>(jsonResult);

                if (searchResult.Pages != null && searchResult.Pages.Count > 0)
                {
                    string wikiNameVideoGame = (options.GameData.Name + " (video game)").RemoveSpecialChars().ToLower().Replace(" ", "");
                    string wikiName = options.GameData.Name.RemoveSpecialChars().ToLower().Replace(" ", "");
                    string wikiStart = wikiName.Substring(0, (wikiName.Length > 5) ? 5 : wikiName.Length);


                    if (options.IsBackgroundDownload)
                    {
                        // TODO: Maybe look for existing wikipedia link and use that as the base!


                        Page foundPage = searchResult.Pages.Where(p => p.KeyMatch == wikiNameVideoGame).FirstOrDefault() ??
                            searchResult.Pages.Where(p => p.KeyMatch == options.GameData.Name.RemoveSpecialChars().ToLower().Replace(" ", "")).FirstOrDefault();

                        if (foundPage != null)
                        {
                            key = foundPage.Key;
                        }
                        else
                        {
                            key = string.Empty;
                        }
                    }
                    else
                    {

                        GenericItemOption chosen = plugin.PlayniteApi.Dialogs.ChooseItemWithSearch(null, s =>
                        {
                            return searchResult.Pages.Select(WikipediaItemOption.FromWikipediaSearchResult)
                                .OrderByDescending(o => o.Description != null && o.Description.Contains("video game"))
                                .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").StartsWith(wikiNameVideoGame))
                                .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").StartsWith(wikiStart))
                                .ThenByDescending(o => o.Name.RemoveSpecialChars().ToLower().Replace(" ", "").Contains(wikiName))
                                .ToList<GenericItemOption>();
                        }, options.GameData.Name, "Wikipedia: select game");


                        if (chosen != null)
                        {
                            key = ((WikipediaItemOption)chosen).Key;
                        }
                    }

                    if (key != string.Empty)
                    {
                        apiUrl = string.Format(PageUrl, key.UrlEncode());

                        jsonResult = client.DownloadString(apiUrl);

                        page = Serialization.FromJson<WikipediaGameData>(jsonResult);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from Wikipedia");
            }

            return foundGame = new WikipediaGameMetadata(page);







            /*       var deflatedSearchGameName = options.GameData.Name.Deflate();
            List<string> platformSpecs = options.GameData.Platforms?.Where(p => p.SpecificationId != null).Select(p => p.SpecificationId).ToList();
            List<string> platformNames = options.GameData.Platforms?.Select(p => p.Name).ToList();
            foreach (var game in results)
            {
                var deflatedMatchedGameName = game.MatchedName.Deflate();
                if (!deflatedSearchGameName.Equals(deflatedMatchedGameName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (options.GameData.Platforms?.Count > 0)
                {
                    var platforms = platformUtility.GetPlatforms(game.Platform);
                    foreach (var platform in platforms)
                    {
                        if (platform is MetadataSpecProperty specPlatform && platformSpecs.Contains(specPlatform.Id))
                        {
                            return foundGame = game;
                        }
                        else if (platform is MetadataNameProperty namePlatform && platformNames.Contains(namePlatform.Name))
                        {
                            return foundGame = game;
                        }
                    }
                }
                else
                {
                    //no platforms to match, so consider a name match a success
                    return foundGame = game;
                }
            }*/

        }

        public override string GetName(GetMetadataFieldArgs args)
        {
            return FindGame().Name ?? base.GetName(args);
        }
        public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args)
        {
            return FindGame().ReleaseDate ?? base.GetReleaseDate(args);
        }
        public override IEnumerable<MetadataProperty> GetGenres(GetMetadataFieldArgs args)
        {
            return FindGame().Genres ?? base.GetGenres(args);
        }
        public override IEnumerable<MetadataProperty> GetDevelopers(GetMetadataFieldArgs args)
        {
            return FindGame().Developers ?? base.GetDevelopers(args);
        }
        public override IEnumerable<MetadataProperty> GetPublishers(GetMetadataFieldArgs args)
        {
            return FindGame().Publishers ?? base.GetPublishers(args);
        }
        public override IEnumerable<MetadataProperty> GetFeatures(GetMetadataFieldArgs args)
        {
            return FindGame().Features ?? base.GetFeatures(args);
        }
        public override IEnumerable<MetadataProperty> GetTags(GetMetadataFieldArgs args)
        {
            return FindGame().Tags ?? base.GetTags(args);
        }
        public override IEnumerable<Link> GetLinks(GetMetadataFieldArgs args)
        {
            return FindGame().Links ?? base.GetLinks(args);
        }
        public override IEnumerable<MetadataProperty> GetSeries(GetMetadataFieldArgs args)
        {
            return FindGame().Series ?? base.GetSeries(args);
        }
        public override IEnumerable<MetadataProperty> GetPlatforms(GetMetadataFieldArgs args)
        {
            return FindGame().Platforms ?? base.GetPlatforms(args);
        }
        public override string GetDescription(GetMetadataFieldArgs args)
        {
            return new DescriptionParser(FindGame().Key).Description ?? base.GetDescription(args);
        }
    }
}