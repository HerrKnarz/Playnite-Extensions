using KNARZhelper;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to IGN.
    /// </summary>
    internal class LinkIgnGuides : BaseClasses.Linker
    {
        public override string BaseUrl => "https://www.ign.com/wikis/";
        public override string LinkName => "IGN Guide";

        // Since IGN Guides always returns an url, even if that leads to a non-existing guide, we also check if the title isn't the one
        // of that generic page.
        public override string WrongTitle => "Guide - IGN";

        // IGN Links need the result name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("_", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();
    }
}