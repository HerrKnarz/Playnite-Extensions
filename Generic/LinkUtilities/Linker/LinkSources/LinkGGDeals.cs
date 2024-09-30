using KNARZhelper;
using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to GG.deals.
    /// </summary>
    internal class LinkGgDeals : BaseClasses.Linker
    {
        private const string _standardUrl = "https://gg.deals/game/";
        private const string _steamUrl = "https://gg.deals/steam/app/";
        private string _baseUrl;
        public override string BaseUrl => _baseUrl;
        public override string BrowserSearchUrl => "https://gg.deals/games/?title=";
        public override string LinkName => "GG.deals";

        // GG.deals Links need the game name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
        {
            // IsThereAnyDeal provides links to steam games directly via the game id.
            if (game.PluginId == Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab"))
            {
                _baseUrl = _steamUrl;
                return game.GameId;
            }

            // For all other libraries links need the result name in lowercase without special characters and white spaces with numbers translated to roman numbers.
            _baseUrl = _standardUrl;

            return (gameName ?? game.Name).RemoveSpecialChars(" ")
                .Replace("_", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();
        }
    }
}