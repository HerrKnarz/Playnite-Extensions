using KNARZhelper;
using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to Doom Wiki.
    /// </summary>
    internal class LinkDoomWiki : BaseClasses.Linker
    {
        public override string BaseUrl => "https://doomwiki.org/wiki/";
        public override string BrowserSearchUrl => "https://doomwiki.org/w/index.php?title=Special%3ASearch&profile=default&fulltext=Search&search=";
        public override string LinkName => "Doom Wiki";
        public override string SearchUrl => "https://doomwiki.org/w/api.php?action=opensearch&format=xml&search={0}&limit=50";

        // Doom Wiki Links need the game with underscores instead of whitespaces and special
        // characters simply encoded.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).CollapseWhitespaces()
                .Replace(" ", "_")
                .EscapeDataString();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new List<GenericItemOption>(ParseHelper.GetMediaWikiResultsFromApi(SearchUrl, searchTerm, LinkName, this));
    }
}