using Playnite.SDK.Models;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to MobyGames if one is found using the game name.
    /// </summary>
    class LinkPCGamingWiki : Link
    {
        private string gameName = string.Empty;
        public override string LinkName { get; } = "PCGamingWiki";
        public override string BaseUrl { get; } = "https://www.pcgamingwiki.com/wiki/{0}";

        public override bool AddLink(Game game)
        {
            if (!LinkHelper.LinkExists(game, LinkName))
            {
                // PCGamingWiki links need the game simply encoded.
                gameName = System.Uri.EscapeDataString(game.Name);

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
            else
            {
                return false;
            }
        }
        public LinkPCGamingWiki(LinkUtilitiesSettings settings) : base(settings)
        {
        }
    }
}