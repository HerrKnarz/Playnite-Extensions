using HtmlAgilityPack;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Lemon Amiga.
    /// </summary>
    class LinkLemonAmiga : Link
    {
        public override string LinkName { get; } = "Lemon Amiga";
        public override LinkAddTypes AddType { get; } = LinkAddTypes.SingleSearchResult;
        public override string SearchUrl { get; } = "https://www.lemonamiga.com/games/list.php?list_title=";

        public override string BaseUrl { get; } = "https://www.lemonamiga.com/games/";

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.RemoveSpecialChars().CollapseWhitespaces().Replace(" ", "+").ToLower()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'game-col')]");

                if (htmlNodes != null && htmlNodes.Count > 0)
                {
                    int counter = 0;

                    foreach (HtmlNode node in htmlNodes)
                    {
                        counter++;
                        string suffix = string.Empty;

                        HtmlNode suffixNode = node.SelectSingleNode("./div/div[@class='game-grid-title']/a/img");

                        if (suffixNode != null)
                        {
                            suffix = $" ({suffixNode.GetAttributeValue("alt", "")})";
                        }

                        SearchResults.Add(new SearchResult
                        {
                            Name = $"{counter}. {WebUtility.HtmlDecode(node.SelectSingleNode("./div/div[@class='game-grid-title']/a").InnerText)}{suffix}",
                            Url = $"{BaseUrl}{node.SelectSingleNode("./div/div[@class='game-grid-title']/a").GetAttributeValue("href", "")}",
                            Description = $"{WebUtility.HtmlDecode(node.SelectSingleNode("./div/div[@class='game-grid-info']").InnerText)}{WebUtility.HtmlDecode(node.SelectSingleNode("./div/div[@class='game-grid-category']").InnerText)}"
                        }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.SearchLink(searchTerm);
        }

        public LinkLemonAmiga(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}