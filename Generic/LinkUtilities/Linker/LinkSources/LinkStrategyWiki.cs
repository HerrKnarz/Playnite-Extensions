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
    internal class LinkStrategyWiki : BaseClasses.Linker
    {
        public override string LinkName { get; } = "StrategyWiki";
        public override string BaseUrl { get; } = "https://strategywiki.org/wiki/";
        public override string SearchUrl { get; } = "https://strategywiki.org/w/index.php?search={0}&fulltext=1";

        private readonly string _websiteUrl = "https://strategywiki.org";

        // StrategyWiki Links need the game with underscores instead of whitespaces and special characters simply encoded.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).CollapseWhitespaces()
                .Replace(" ", "_")
                .EscapeDataString();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new List<GenericItemOption>(ParseHelper.GetMediaWikiResultsFromHtml(SearchUrl, searchTerm, _websiteUrl, LinkName));

        public LinkStrategyWiki() : base()
        {
        }
    }
}
