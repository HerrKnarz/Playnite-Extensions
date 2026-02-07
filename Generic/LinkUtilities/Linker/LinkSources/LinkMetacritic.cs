using KNARZhelper;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to Metacritic.
    /// </summary>
    internal class LinkMetacritic : BaseClasses.Linker
    {
        public override string BaseUrl => "https://www.metacritic.com/game/";
        public override string BrowserSearchUrl => "https://www.metacritic.com/search/{0}/?page=1&category=13";
        public override string LinkName => "Metacritic";
        public override string SearchUrl => string.Empty;

        public override string GetBrowserSearchLink(Game game = null) => string.Format(BrowserSearchUrl, game.Name.RemoveDiacritics().EscapeDataString());

        // Metacritic Links need the game name in lowercase without special characters and hyphens
        // instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower() + "/";
    }
}