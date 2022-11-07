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