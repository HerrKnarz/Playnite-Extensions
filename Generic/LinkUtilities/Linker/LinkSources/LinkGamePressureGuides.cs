using KNARZhelper;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to IGN.
    /// </summary>
    internal class LinkGamePressureGuides : BaseClasses.Linker
    {
        public override string BaseUrl => "https://www.gamepressure.com/";
        public override string LinkName => "gamepressure Guides";
        public override bool ReturnsSameUrl { get; set; } = true;

        // IGN Links need the result name in lowercase without special characters and hyphens
        // instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("_", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower() + '/';
    }
}