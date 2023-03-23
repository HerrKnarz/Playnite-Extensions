using KNARZhelper;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Map Genie.
    /// </summary>
    internal class LinkMapGenie : Link
    {
        public override string LinkName { get; } = "Map Genie";
        public override string BaseUrl { get; } = "https://mapgenie.io/";

        // Map Genie Links need the game name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("_", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public LinkMapGenie(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
