using KNARZhelper;
using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to Sega Retro.
    /// </summary>
    internal class LinkSegaRetro : BaseClasses.Linker
    {
        private const string _websiteUrl = "https://segaretro.org";
        public override string LinkName => "Sega Retro";
        public override string BaseUrl => "https://segaretro.org/";
        public override string SearchUrl => "https://segaretro.org/index.php?search={0}&fulltext=1";
        public override string BrowserSearchUrl => "https://segaretro.org/index.php?search=";

        // Sega Retro Links need the game with underscores instead of whitespaces and special characters simply encoded.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).CollapseWhitespaces()
                .Replace(" ", "_")
                .EscapeDataString();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new List<GenericItemOption>(ParseHelper.GetMediaWikiResultsFromHtml(SearchUrl, searchTerm, _websiteUrl, LinkName, 2));
    }
}