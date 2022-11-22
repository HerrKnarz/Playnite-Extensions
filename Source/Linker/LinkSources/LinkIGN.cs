using LinkUtilities.Helper;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to IGN.
    /// </summary>
    class LinkIGN : Link
    {
        public override string LinkName { get; } = "IGN";
        public override string BaseUrl { get; } = "https://www.ign.com/games/";

        public override string GetGamePath(Game game)
        {
            // IGN Links need the result name in lowercase without special characters and hyphens instead of white spaces.
            return game.Name
                .RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("_", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();
        }

        public LinkIGN(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}