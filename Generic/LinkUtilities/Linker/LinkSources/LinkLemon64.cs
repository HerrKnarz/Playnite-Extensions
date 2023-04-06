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
    /// Adds a link to Lemon Amiga.
    /// </summary>
    internal class LinkLemon64 : BaseClasses.Linker
    {
        private const string _websiteUrl = "https://www.lemon64.com";
        public override string LinkName => "Lemon64";
        public override string SearchUrl => "https://www.lemon64.com/games/list.php?list_title=";
        public override string BaseUrl => _websiteUrl + "/game/";

        // Lemon64 Links need the game name in lowercase without leading articles, special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars()
                .RemoveFirst("the ")
                .RemoveFirst("a ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.RemoveSpecialChars().CollapseWhitespaces().Replace(" ", "+").ToLower()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'game-col')]/div/div[2]");

                if (htmlNodes?.Any() ?? false)
                {
                    return htmlNodes.Select(node
                        => new SearchResult
                        {
                            Name = $"{WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='game-grid-title']").InnerText.Replace("\n", " ").Remove(0, 1))}",
                            Url = $"{_websiteUrl}{node.SelectSingleNode("./div[@class='game-grid-title']/a").GetAttributeValue("href", "")}",
                            Description = $"{WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='grid-info']").InnerText)}{WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='grid-category']").InnerText)}"
                        }).Cast<GenericItemOption>().ToList();
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