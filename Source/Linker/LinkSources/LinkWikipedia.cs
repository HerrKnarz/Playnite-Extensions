using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Wikipedia.
    /// </summary>
    class LinkWikipedia : Link
    {
        public override string LinkName { get; } = "Wikipedia";
        public override string BaseUrl { get; } = "https://en.wikipedia.org/wiki/";
        public override string SearchUrl { get; } = "https://en.wikipedia.org/w/api.php?action=opensearch&format=xml&search={0}&limit=50";

        public override string GetGamePath(Game game)
        {
            // Wikipedia Links need the game with underscores instead of whitespaces and special characters simply encoded.
            return game.Name.CollapseWhitespaces().Replace(" ", "_").EscapeDataString();
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults = ParseHelper.GetMediaWikiResultsFromApi(SearchUrl, searchTerm, LinkName);

            return base.SearchLink(searchTerm);
        }

        public LinkWikipedia(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}