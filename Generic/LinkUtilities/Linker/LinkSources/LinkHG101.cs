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
    /// Adds a link to Hardcore Gaming 101.
    /// </summary>
    class LinkHG101 : Link
    {
        public override string LinkName { get; } = "Hardcore Gaming 101";
        public override string BaseUrl { get; } = "http://www.hardcoregaming101.net/";
        public override string SearchUrl { get; } = "http://www.hardcoregaming101.net/?s=";

        public override string GetGamePath(Game game)
        {
            // HG101 Links need the game name in lowercase without special characters and hyphens instead of white spaces.
            return game.Name.RemoveSpecialChars().CollapseWhitespaces().Replace(" ", "-").ToLower();
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//header[@class='entry-header']");

                if (htmlNodes != null && htmlNodes.Count > 0)
                {
                    foreach (HtmlNode node in htmlNodes)
                    {
                        HtmlNodeCollection reviewNodes = node.SelectNodes("./div[@class='index-entry-meta']/div[a='Review']");

                        if (reviewNodes != null && reviewNodes.Count > 0)
                        {
                            SearchResults.Add(new SearchResult
                            {
                                Name = WebUtility.HtmlDecode(node.SelectSingleNode("./h2/a").InnerText),
                                Url = node.SelectSingleNode("./h2/a").GetAttributeValue("href", ""),
                                Description = WebUtility.HtmlDecode(node.SelectNodes("./div[@class='index-entry-meta']/div/a").Select(tagNode => tagNode.InnerText).Aggregate((total, part) => total + ", " + part))
                            });
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.SearchLink(searchTerm);
        }

        public LinkHG101(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}