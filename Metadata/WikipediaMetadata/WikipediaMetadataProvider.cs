using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    public class WikipediaMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions options;
        private readonly WikipediaMetadata plugin;

        private string baseUrl = "https://en.wikipedia.org/w/rest.php/v1/";
        private string searchUrl { get => baseUrl + "search/page?q={0}&limit={1}"; }
        private string pageUrl { get => baseUrl + "page/{0}"; }

        private WikipediaGameData foundGame;

        public override List<MetadataField> AvailableFields => throw new NotImplementedException();

        public WikipediaMetadataProvider(MetadataRequestOptions options, WikipediaMetadata plugin)
        {
            this.options = options;
            this.plugin = plugin;
        }

        private WikipediaGameData FindGame()
        {
            if (foundGame != null)
            {
                return foundGame;
            }

            try
            {
                string apiUrl = string.Format(searchUrl, options.GameData.Name.UrlEncode(), 1);

                WebClient client = new WebClient();

                string jsonResult = client.DownloadString(apiUrl);

                string key = string.Empty;

                WikipediaSearchResult searchResult = Serialization.FromJson<WikipediaSearchResult>(jsonResult);

                if (searchResult.Pages != null && searchResult.Pages.Count > 0)
                {
                    if (options.IsBackgroundDownload)
                    {
                        key = searchResult.Pages[0].Key;
                    }
                    else
                    {
                        GenericItemOption chosen = plugin.PlayniteApi.Dialogs.ChooseItemWithSearch(null, s =>
                        {
                            return searchResult.Pages.Select(WikipediaItemOption.FromWikipediaSearchResult).ToList<GenericItemOption>();
                        }, options.GameData.Name, "Wikipedia: select game");


                        if (chosen != null)
                        {
                            key = ((WikipediaItemOption)chosen).Key;
                        }
                    }

                    if (key != string.Empty)
                    {
                        apiUrl = string.Format(pageUrl, key.UrlEncode());

                        jsonResult = client.DownloadString(apiUrl);

                        return foundGame = Serialization.FromJson<WikipediaGameData>(jsonResult);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from Wikipedia");
            }

            return foundGame = new WikipediaGameData();







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
            return FindGame().Title ?? base.GetName(args);
        }
        public override string GetDescription(GetMetadataFieldArgs args)
        {
            return FindGame().Source ?? base.GetDescription(args);
        }
    }
}