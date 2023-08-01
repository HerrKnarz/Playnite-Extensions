using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    ///     Adds a link to Backloggd.
    /// </summary>
    internal class LinkBackloggd : BaseClasses.Linker
    {
        private const string _websiteUrl = "https://www.backloggd.com";

        public override string LinkName => "Backloggd";
        public override string BaseUrl => _websiteUrl + "/games/";
        public override string SearchUrl => _websiteUrl + "/search/games/";

        // Since Backloggd always returns the status code OK and the same url, even if that leads to a non existing game, we also check if the title isn't the one
        // of that generic page. Funny enough it says 404 but doesn't return that status code...
        public override string WrongTitle => "404 Not Found";

        // Backloggd Links need the game name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'result')]");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode(".//div[contains(@class, 'game-name')]/a/h3").InnerText),
                        Url = $"{_websiteUrl}{n.SelectSingleNode(".//div[contains(@class, 'game-name')]/a").GetAttributeValue("href", "")}",
                        Description = WebUtility.HtmlDecode(n.SelectSingleNode(".//h1[contains(@class, 'game-date')]").InnerText)
                    }));
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