using KNARZhelper;

using Playnite.SDK.Models;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to IGN.
    /// </summary>
    internal class LinkIGNGuides : BaseClasses.Linker
    {
        public override string LinkName { get; } = "IGN Guide";
        public override string BaseUrl { get; } = "https://www.ign.com/wikis/";

        // IGN Links need the result name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("_", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public LinkIGNGuides() : base()
        {
        }
    }
}