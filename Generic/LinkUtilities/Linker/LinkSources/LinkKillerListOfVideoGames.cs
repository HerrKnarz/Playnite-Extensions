using KNARZhelper;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker.LinkSources
{
    internal class LinkKillerListOfVideoGames : BaseClasses.Linker
    {
        private const string _baseUrl = "https://www.arcade-museum.com/Videogame/";
        public override string BaseUrl => _baseUrl;

        public override string LinkName => "Killer List Of Video Games";

        public override string GetGamePath(Game game, string gameName = null) => (gameName ?? game.Name)
            .RemoveDiacritics()
            .RemoveSpecialChars()
            .Replace("-", " ")
            .CollapseWhitespaces()
            .Replace(" ", "-")
            .ToLower();
    }
}
