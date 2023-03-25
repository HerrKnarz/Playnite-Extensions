using KNARZhelper;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to IGN.
    /// </summary>
    internal class LinkIGN : BaseClasses.Link
    {
        public override string LinkName { get; } = "IGN";
        public override string BaseUrl { get; } = "https://www.ign.com/games/";

        // IGN Links need the result name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("_", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public LinkIGN() : base()
        {
        }
    }
}