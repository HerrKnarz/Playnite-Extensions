using KNARZhelper;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to TV Tropes.
    /// </summary>
    internal class LinkTvTropes : BaseClasses.Linker
    {
        private const string _baseUrl = "https://tvtropes.org/pmwiki/pmwiki.php/VideoGame/";
        public override string BaseUrl => _baseUrl;

        public override string BrowserSearchUrl => "https://tvtropes.org/pmwiki/search_result.php?q=";

        public override string LinkName => "TV Tropes";

        public override string GetBrowserSearchLink(string searchTerm) => $"{BrowserSearchUrl}{searchTerm.RemoveDiacritics().EscapeDataString()}";

        // TVTropes Links need the game name in title case without diacritics exchanged and special characters and white spaces removed.
        public override string GetGamePath(Game game, string gameName = null) => (gameName ?? game.Name)
            .RemoveDiacritics().ToTitleCase().RemoveSpecialChars().Replace("_", "").Replace(" ", "");
    }
}