using HtmlAgilityPack;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to PCGamingWiki.
    /// </summary>
    class LinkPCGamingWiki : Link
    {
        public override string LinkName { get; } = "PCGamingWiki";
        public override string BaseUrl { get; } = "https://www.pcgamingwiki.com/wiki/";
        public override string SearchUrl { get; } = "https://www.pcgamingwiki.com/w/index.php?search={0}&fulltext=1";

        internal string WebsiteUrl = "https://www.pcgamingwiki.com";

        public override string GetGamePath(Game game)
        {
            // PCGamingWiki Links need the game simply encoded.
            return game.Name.EscapeDataString();
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            // We could use the Media Wiki API here, but unfortunately the search results aren't good enough because the version on the
            // PCGamingWiki doesn't support search profiles yet.

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
                            counter++;

                            SearchResults.Add(new SearchResult
                            {
                                Name = $"{counter}. {node.SelectSingleNode("./div[@class='mw-search-result-heading']").InnerText}",
                                Url = WebsiteUrl + node.SelectSingleNode("./div[@class='mw-search-result-heading']/a").GetAttributeValue("href", ""),
                                Description = ""
                            }
                            );
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

        public LinkPCGamingWiki(LinkUtilitiesSettings settings) : base(settings)
        {
        }
    }
}