using HtmlAgilityPack;
using LinkUtilities.Helper;
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
    /// Adds a link to StrategyWiki.
    /// </summary>
    class LinkStrategyWiki : Link
    {
        public override string LinkName { get; } = "StrategyWiki";
        public override string BaseUrl { get; } = "https://strategywiki.org/wiki/";
        public override string SearchUrl { get; } = "https://strategywiki.org/w/index.php?search={0}&fulltext=1";

        internal string WebsiteUrl = "https://strategywiki.org";

        public override string GetGamePath(Game game)
        {
            // StrategyWiki Links need the game with underscores instead of whitespaces and special characters simply encoded.
            return game.Name.CollapseWhitespaces().Replace(" ", "_").EscapeDataString();
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            // We could use the Media Wiki API here, but unfortunately the search results aren't good enough because the version on the
            // StrategyWiki doesn't support search profiles yet.

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(string.Format(SearchUrl, searchTerm.UrlEncode()));

                HtmlNode resultNode = doc.DocumentNode.SelectSingleNode("//div[@class='searchresults']");

                if (resultNode.SelectSingleNode("./h2/span[text() = 'Page title matches']") != null)
                {
                    HtmlNodeCollection htmlNodes = resultNode.SelectSingleNode("./ul[@class='mw-search-results']").SelectNodes("./li[@class='mw-search-result']");

                    if (htmlNodes != null && htmlNodes.Count > 0)
                    {
                        int counter = 0;

                        foreach (HtmlNode node in htmlNodes)
                        {
                            string url = node.SelectSingleNode("./div[@class='mw-search-result-heading']/a").GetAttributeValue("href", "");

                            // Sega Retro returns subbages to games in the results, so we simply count the shashes to filter them out.
                            if (url.Count(x => x == '/') < 3)
                            {
                                string redirect = (node.SelectSingleNode("./div[@class='searchresult']").InnerText.StartsWith("#REDIRECT")) ? "(REDIRECT) " : "";

                                counter++;

                                SearchResults.Add(new SearchResult
                                {
                                    Name = $"{counter}. {WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='mw-search-result-heading']").InnerText)}",
                                    Url = WebsiteUrl + url,
                                    Description = redirect + WebsiteUrl + url
                                }
                                );
                            }
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

        public LinkStrategyWiki(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
