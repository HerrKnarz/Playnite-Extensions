using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Sega Retro.
    /// </summary>
    class LinkSegaRetro : Link
    {
        public override string LinkName { get; } = "Sega Retro";
        public override string BaseUrl { get; } = "https://segaretro.org/";
        public override string SearchUrl { get; } = "https://segaretro.org/index.php?search={0}&fulltext=1";

        internal string WebsiteUrl = "https://segaretro.org";

        public override string GetGamePath(Game game)
        {
            // Sega Retro Links need the game with underscores instead of whitespaces and special characters simply encoded.
            return game.Name.CollapseWhitespaces().Replace(" ", "_").EscapeDataString();
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults = ParseHelper.GetMediaWikiResultsFromHtml(SearchUrl, searchTerm, WebsiteUrl, LinkName, 2);

            return base.SearchLink(searchTerm);
        }

        public LinkSegaRetro(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
