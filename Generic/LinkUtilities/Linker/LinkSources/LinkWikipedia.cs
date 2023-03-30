using KNARZhelper;
using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Wikipedia.
    /// </summary>
    internal class LinkWikipedia : BaseClasses.Linker
    {
        public override string LinkName { get; } = "Wikipedia";
        public override string BaseUrl { get; } = "https://en.wikipedia.org/wiki/";
        public override string SearchUrl { get; } = "https://en.wikipedia.org/w/api.php?action=opensearch&format=xml&search={0}&limit=50";

        // Wikipedia Links need the game with underscores instead of whitespaces and special characters simply encoded.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).CollapseWhitespaces()
                .Replace(" ", "_")
                .EscapeDataString();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new List<GenericItemOption>(ParseHelper.GetMediaWikiResultsFromApi(SearchUrl, searchTerm, LinkName));

        public LinkWikipedia() : base()
        {
        }
    }
}