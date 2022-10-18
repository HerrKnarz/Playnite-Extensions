using Playnite.SDK.Models;
using System.Text.RegularExpressions;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to MobyGames if one is found using the game name.
    /// </summary>
    class LinkMobyGames : Link
    {
        private string gameName = string.Empty;
        public override string LinkName { get; } = "MobyGames";
        public override string BaseUrl { get; } = "https://www.mobygames.com/game/{0}";

        public override bool AddLink(Game game)
        {
            // MobyGames links need the game name in lowercase without special characters and hyphens instead of white spaces.
            gameName = Regex.Replace(game.Name, @"[^a-zA-Z0-9\-\s]+", " ");
            // We use Regex.Replace twice, because after the first run there could be new instances of multiple spaces in a row, that wew
            // wouldn't catch otherwise.
            gameName = Regex.Replace(gameName, @"\s+", " ").
                Trim().
                Replace(" ", "-").
                ToLower();

            LinkUrl = string.Format(BaseUrl, gameName);

            if (LinkHelper.CheckUrl(LinkUrl))
            {
                return LinkHelper.AddLink(game, LinkName, LinkUrl, Settings);
            }
            else
            {
                LinkUrl = string.Empty;

                return false;
            }
        }
        public LinkMobyGames(LinkUtilitiesSettings settings) : base(settings)
        {
        }
    }
}