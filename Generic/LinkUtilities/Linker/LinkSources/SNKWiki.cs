using KNARZhelper;
using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to SNK Wiki.
    /// </summary>
    class LinkSNKWiki : Link
    {
        public override string LinkName { get; } = "SNK Wiki";
        public override string BaseUrl { get; } = "https://snk.fandom.com/wiki/";
        public override string SearchUrl { get; } = "https://snk.fandom.com/api.php?action=opensearch&format=xml&search={0}&limit=50";

        public override string GetGamePath(Game game)
        {
            // Nintendo Wiki Links need the game with underscores instead of whitespaces and special characters simply encoded.
            return game.Name.CollapseWhitespaces().Replace(" ", "_").EscapeDataString();
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults = ParseHelper.GetMediaWikiResultsFromApi(SearchUrl, searchTerm, LinkName);

            return base.SearchLink(searchTerm);
        }

        public LinkSNKWiki(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
