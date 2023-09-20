using KNARZhelper;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker
{
    /// <summary>
    ///     Adds a link to GG.deals.
    /// </summary>
    internal class LinkGGDeals : BaseClasses.Linker
    {
        public override string LinkName => "GG.deals";
        public override string BaseUrl => "https://gg.deals/game/";

        // GG.deals Links need the game name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars(" ")
                .Replace("_", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();
    }
}