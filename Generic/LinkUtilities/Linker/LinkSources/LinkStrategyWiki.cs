using KNARZhelper;
using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

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

        public override string GetGamePath(Game game, string gameName = null)
        {
            // StrategyWiki Links need the game with underscores instead of whitespaces and special characters simply encoded.
            return (gameName ?? game.Name).CollapseWhitespaces().Replace(" ", "_").EscapeDataString();
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults = ParseHelper.GetMediaWikiResultsFromHtml(SearchUrl, searchTerm, WebsiteUrl, LinkName);

            return base.SearchLink(searchTerm);
        }

        public LinkStrategyWiki(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
