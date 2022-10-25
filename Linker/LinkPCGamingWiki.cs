using HtmlAgilityPack;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to MobyGames if one is found using the game name.
    /// </summary>
    class LinkPCGamingWiki : Link
    {
        public override string LinkName { get; } = "PCGamingWiki";
        public override string BaseUrl { get; } = "https://www.pcgamingwiki.com/wiki/{0}";
        public override string SearchUrl { get; } = "https://www.pcgamingwiki.com/w/index.php?search={0}&fulltext=1";

        internal string WebsiteUrl = "https://www.pcgamingwiki.com";

        public override bool AddLink(Game game)
        {
            if (!LinkHelper.LinkExists(game, LinkName))
            {
                // PCGamingWiki Links need the game simply encoded.
                string gameName = System.Uri.EscapeDataString(game.Name);

                LinkUrl = string.Format(BaseUrl, gameName);

                if (LinkHelper.CheckUrl(LinkUrl))
                {
                    return LinkHelper.AddLink(game, LinkName, LinkUrl, Settings);
                }
                else
                {
                    LinkUrl = string.Empty;

                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            searchTerm = WebUtility.UrlEncode(searchTerm);

            // We could use the Media Wiki API here, but unfortunately the search results aren't good enough because the version on the
            // PCGamingWiki doesn't support search profiles yet.

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(string.Format(SearchUrl, searchTerm));

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
                            Name = counter.ToString() + ". " + node.SelectSingleNode("./div[@class='mw-search-result-heading']").InnerText,
                            Url = WebsiteUrl + node.SelectSingleNode("./div[@class='mw-search-result-heading']/a").GetAttributeValue("href", ""),
                            Description = ""
                        }
                        );
                    }
                }
            }

            return base.SearchLink(searchTerm);
        }

        public LinkPCGamingWiki(LinkUtilitiesSettings settings) : base(settings)
        {
        }
    }
}