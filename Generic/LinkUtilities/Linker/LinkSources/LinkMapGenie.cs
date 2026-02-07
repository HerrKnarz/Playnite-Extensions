using KNARZhelper;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to Map Genie.
    /// </summary>
    internal class LinkMapGenie : BaseClasses.Linker
    {
        public override string BaseUrl => "https://mapgenie.io/";
        public override int Delay => 200;
        public override string LinkName => "Map Genie";

        // Map Genie Links need the game name in lowercase without special characters and hyphens
        // instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
        {
            return (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("_", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();
        }
    }
}