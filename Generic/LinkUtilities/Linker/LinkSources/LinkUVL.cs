using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker.LinkSources
{
    internal class LinkUVL : BaseClasses.Linker
    {
        private const string _websiteUrl = "https://www.uvlist.net";
        private readonly PlatformHelper _platformHelper = new(API.Instance.Database.Platforms);
        public override string LinkName => "UVL";
        public override string SearchUrl => $"{_websiteUrl}/globalsearch/?t=";

        public override string GetGamePath(Game game, string gameName = null)
        {
            var searchName = gameName ?? game.Name;

            if (game == null || searchName.IsNullOrEmpty())
            {
                return string.Empty;
            }

            var compareName = searchName.NormalizeSearchTerm();
            searchName = searchName.RemoveEditionSuffix();

            var searchResults = GetSearchResults(searchName);

            searchName = searchName.NormalizeSearchTerm();

            var result =
                searchResults?.FirstOrDefault(p => p.Name.NormalizeSearchTerm().Equals(compareName, StringComparison.InvariantCultureIgnoreCase)
                   && game.Platforms.Any(gp => gp.SpecificationId == ((SearchResult)p).PlatformSpecId || gp.Name == ((SearchResult)p).PlatformName))
                ?? searchResults?.FirstOrDefault(p => p.Name.NormalizeSearchTerm().Equals(searchName, StringComparison.InvariantCultureIgnoreCase)
                    && game.Platforms.Any(gp => gp.SpecificationId == ((SearchResult)p).PlatformSpecId || gp.Name == ((SearchResult)p).PlatformName))
                ?? searchResults?.FirstOrDefault(p => p.Name.NormalizeSearchTerm().Equals(compareName, StringComparison.InvariantCultureIgnoreCase))
                ?? searchResults?.FirstOrDefault(p => p.Name.NormalizeSearchTerm().Equals(searchName, StringComparison.InvariantCultureIgnoreCase));

            return result is SearchResult uvlResult ? uvlResult.Url : string.Empty;
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            //TODO: merge with the search functionality of the UVL add in P11!
            try
            {
                (var success, var document) = LoadDocument($"{SearchUrl}{searchTerm.UrlEncode()}", string.Empty, true);

                if (!success)
                {
                    return null;
                }

                var cards = document.DocumentNode.SelectNodes("//div[contains(@class, 'card')]");

                if (cards == null)
                {
                    return null;
                }

                HtmlNodeCollection results = null;

                foreach (var card in cards)
                {
                    if (card.SelectSingleNode(".//h2[@class='card-header']")?.InnerText == "Games by title")
                    {
                        results = card.SelectNodes(".//tbody/tr");

                        break;
                    }
                }

                if (results?.Any() ?? false)
                {
                    var resultList = new List<GenericItemOption>();

                    foreach (var row in results)
                    {
                        var platformName = WebUtility.HtmlDecode(row.SelectSingleNode("./td[3]/span")?.InnerText);
                        var platformSpecId = string.Empty;

                        if (!platformName.IsNullOrEmpty())
                        {
                            var foundPlatform = _platformHelper.GetPlatforms(platformName).FirstOrDefault();

                            if (foundPlatform != null)
                            {
                                if (foundPlatform is MetadataSpecProperty specProperty)
                                {
                                    var foundPlatformInDb = API.Instance.Database.Platforms.Where(p => p.SpecificationId == specProperty.Id)?
                                        .OrderBy(p => p.Name == "Arcade" ? 0 : 1).ThenBy(p => p.Name).FirstOrDefault();

                                    platformName = foundPlatformInDb?.Name ?? platformName;
                                    platformSpecId = foundPlatformInDb?.SpecificationId ?? platformSpecId;
                                }
                                else if (foundPlatform is MetadataNameProperty nameProperty)
                                {
                                    platformName = nameProperty.Name;
                                }
                            }
                        }

                        resultList.Add(new SearchResult
                        {
                            Name = WebUtility.HtmlDecode(row.SelectSingleNode("./td[1]/a")?.InnerText),
                            Url = _websiteUrl + row.SelectSingleNode("./td[1]/a")?.GetAttributeValue("href", ""),
                            Description = WebUtility.HtmlDecode($"{row.SelectSingleNode("./td[2]")?.InnerText} - {platformName} - {row.SelectSingleNode("./td[4]")?.InnerText}"),
                            PlatformName = platformName,
                            PlatformSpecId = platformSpecId
                        });
                    }

                    return resultList;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.GetSearchResults(searchTerm);
        }
    }
}
