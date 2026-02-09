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
    /// <summary>
    /// Adds a link to Lemon Amiga.
    /// </summary>
    internal class LinkLemonAmiga : BaseClasses.Linker
    {
        private const string _websiteUrl = "https://www.lemonamiga.com";
        public override string BaseUrl => _websiteUrl + "/game/";
        public override string LinkName => "Lemon Amiga";
        public override string SearchUrl => "https://www.lemonamiga.com/games/list.php?list_title=";

        // LemonAmiga Links need the game name in lowercase without leading articles, special
        // characters and hyphens instead of white spaces.
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
                (var success, var document) = LoadDocument($"{SearchUrl}{searchTerm.UrlEncode()}");

                if (!success)
                {
                    return null;
                }

                var htmlNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'game-col')]/div/div[2]");

                if (htmlNodes?.Any() ?? false)
                {
                    var searchResults = new List<GenericItemOption>();

                    foreach (var node in htmlNodes)
                    {
                        var suffixNode = node.SelectSingleNode("./div[@class='game-grid-title']/a/img");
                        var suffix = string.Empty;

                        if (suffixNode != null)
                        {
                            suffix = $" ({suffixNode.GetAttributeValue("alt", "")})";
                        }

                        searchResults.Add(new SearchResult
                        {
                            Name = $"{WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='game-grid-title']/a").InnerText)}{suffix}",
                            Url = $"{_websiteUrl}{node.SelectSingleNode("./div[@class='game-grid-title']/a").GetAttributeValue("href", "")}",
                            Description = $"{WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='grid-info']").InnerText.Trim())} {WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='grid-category']").InnerText.Trim())}"
                        });
                    }

                    return searchResults;
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