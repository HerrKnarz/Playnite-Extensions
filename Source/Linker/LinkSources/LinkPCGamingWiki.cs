using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
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
            // PCGamingWiki Links need the game with underscores instead of whitespaces and special characters simply encoded.
            return game.Name.CollapseWhitespaces().Replace(" ", "_").EscapeDataString();
        }

        public override bool AddLink(Game game)
        {
            LinkUrl = string.Empty;
            bool result = false;

            if (!LinkHelper.LinkExists(game, LinkName))
            {
                string gameName = GetGamePath(game);

                if (!string.IsNullOrEmpty(gameName))
                {
                    if (CheckLink($"{BaseUrl}{gameName}"))
                    {
                        LinkUrl = $"{BaseUrl}{gameName}";
                    }
                    // if the first try didn't find a link, we try it with the capitalized game name. 
                    else
                    {
                        string gameNameCapitalized = game.Name.CollapseWhitespaces().ToTitleCase().Replace(" ", "_").EscapeDataString();

                        if (gameNameCapitalized != gameName && CheckLink($"{BaseUrl}{gameNameCapitalized}"))
                        {
                            LinkUrl = $"{BaseUrl}{gameNameCapitalized}";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(LinkUrl))
                {
                    result = LinkHelper.AddLink(game, LinkName, LinkUrl, Plugin);
                }
            }
            return result;
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults = ParseHelper.GetMediaWikiResultsFromHtml(SearchUrl, searchTerm, WebsiteUrl, LinkName);

            return base.SearchLink(searchTerm);
        }

        public LinkPCGamingWiki(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}