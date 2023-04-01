using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Lemon Amiga.
    /// </summary>
    internal class LinkLemonAmiga : BaseClasses.Linker
    {
        public override string LinkName => "Lemon Amiga";
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string SearchUrl => "https://www.lemonamiga.com/games/list.php?list_title=";

        public override string BaseUrl => "https://www.lemonamiga.com/games/";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.RemoveSpecialChars().CollapseWhitespaces().Replace(" ", "+").ToLower()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'game-col')]");

                if (htmlNodes?.Any() ?? false)
                {
                    List<GenericItemOption> searchResults = new List<GenericItemOption>();

                    foreach (HtmlNode node in htmlNodes)
                    {
                        HtmlNode suffixNode = node.SelectSingleNode("./div/div[@class='game-grid-title']/a/img");

                        string suffix = suffixNode != null
                            ? $" ({suffixNode.GetAttributeValue("alt", "")})"
                            : string.Empty;

                        searchResults.Add(new SearchResult
                        {
                            Name = $"{WebUtility.HtmlDecode(node.SelectSingleNode("./div/div[@class='game-grid-title']/a").InnerText)}{suffix}",
                            Url = $"{BaseUrl}{node.SelectSingleNode("./div/div[@class='game-grid-title']/a").GetAttributeValue("href", "")}",
                            Description = $"{WebUtility.HtmlDecode(node.SelectSingleNode("./div/div[@class='game-grid-info']").InnerText)}{WebUtility.HtmlDecode(node.SelectSingleNode("./div/div[@class='game-grid-category']").InnerText)}"
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