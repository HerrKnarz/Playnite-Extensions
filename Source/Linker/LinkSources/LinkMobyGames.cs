using HtmlAgilityPack;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to MobyGames.
    /// </summary>
    class LinkMobyGames : Link
    {
        public override string LinkName { get; } = "MobyGames";
        public override string BaseUrl { get; } = "https://www.mobygames.com/game/{0}";
        public override string SearchUrl { get; } = "https://www.mobygames.com/search/quick?q={0}";

        public override bool AddLink(Game game)
        {
            if (!LinkHelper.LinkExists(game, LinkName))
            {
                // MobyGames Links need the game name in lowercase without special characters and hyphens instead of white spaces.
                string gameName = game.Name.RemoveSpecialChars().CollapseWhitespaces().Replace(" ", "-").ToLower();

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

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(string.Format(SearchUrl, searchTerm.UrlEncode()));

            HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//div[@class='searchResult']");

            if (htmlNodes != null && htmlNodes.Count > 0)
            {
                foreach (HtmlNode node in htmlNodes)
                {
                    HtmlNode searchData = node.SelectSingleNode("./div[@class='searchData']");

                    SearchResults.Add(new SearchResult
                    {
                        Name = node.SelectSingleNode("./div[@class='searchNumber']").InnerText + " " + searchData.SelectSingleNode("./div[@class='searchTitle']/a").InnerText,
                        Url = searchData.SelectSingleNode("./div[@class='searchTitle']/a").GetAttributeValue("href", ""),
                        Description = searchData.SelectSingleNode("./div[@class='searchDetails']").InnerText
                    }
                    );
                }
            }

            return base.SearchLink(searchTerm);
        }

        public LinkMobyGames(LinkUtilitiesSettings settings) : base(settings)
        {
        }
    }
}