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
    internal class LinkSNKWiki : BaseClasses.Linker
    {
        public override string LinkName { get; } = "SNK Wiki";
        public override string BaseUrl { get; } = "https://snk.fandom.com/wiki/";
        public override string SearchUrl { get; } = "https://snk.fandom.com/api.php?action=opensearch&format=xml&search={0}&limit=50";

        // SNK Wiki Links need the game with underscores instead of whitespaces and special characters simply encoded.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).CollapseWhitespaces()
                .Replace(" ", "_")
                .EscapeDataString();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new List<GenericItemOption>(ParseHelper.GetMediaWikiResultsFromApi(SearchUrl, searchTerm, LinkName));

        public LinkSNKWiki() : base()
        {
        }
    }
}
