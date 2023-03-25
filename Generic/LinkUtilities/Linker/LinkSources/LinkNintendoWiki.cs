using KNARZhelper;
using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Nintendo Wiki.
    /// </summary>
    internal class LinkNintendoWiki : BaseClasses.Link
    {
        public override string LinkName { get; } = "Nintendo Wiki";
        public override string BaseUrl { get; } = "https://nintendo.fandom.com/wiki/";
        public override string SearchUrl { get; } = "https://nintendo.fandom.com/api.php?action=opensearch&format=xml&search={0}&limit=50";

        // Nintendo Wiki Links need the game with underscores instead of whitespaces and special characters simply encoded.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).CollapseWhitespaces()
                .Replace(" ", "_")
                .EscapeDataString();

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults = ParseHelper.GetMediaWikiResultsFromApi(SearchUrl, searchTerm, LinkName);

            return base.SearchLink(searchTerm);
        }

        public LinkNintendoWiki(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
