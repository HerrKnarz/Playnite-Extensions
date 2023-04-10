using KNARZhelper;
using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to PCGamingWiki.
    /// </summary>
    internal class LinkPcGamingWiki : BaseClasses.Linker
    {
        private const string WebsiteUrl = "https://www.pcgamingwiki.com";
        public override string LinkName => "PCGamingWiki";
        public override string BaseUrl => $"{WebsiteUrl}/wiki/";
        public override string SearchUrl => WebsiteUrl + "/w/index.php?search={0}&fulltext=1";

        // PCGamingWiki Links need the game with underscores instead of whitespaces and special characters simply encoded.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).CollapseWhitespaces()
                .Replace(" ", "_")
                .EscapeDataString();

        public override bool FindLinks(Game game, out List<Link> links)
        {
            LinkUrl = string.Empty;
            links = new List<Link>();

            if (LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            string gameName = GetGamePath(game, game.Name.RemoveEditionSuffix());

            if (string.IsNullOrEmpty(gameName))
            {
                return false;
            }

            if (CheckLink($"{BaseUrl}{gameName}"))
            {
                LinkUrl = $"{BaseUrl}{gameName}";
            }
            // if the first try didn't find a link, we try it with the capitalized game name.
            else
            {
                string gameNameCapitalized = game.Name.CollapseWhitespaces().ToTitleCase().Replace(" ", "_").EscapeDataString();

                if (gameNameCapitalized != gameName && CheckLink($"{BaseUrl}{gameNameCapitalized}"))
                {
                    LinkUrl = $"{BaseUrl}{gameNameCapitalized}";
                }
            }

            if (string.IsNullOrEmpty(LinkUrl))
            {
                return false;
            }

            links.Add(new Link(LinkName, LinkUrl));

            return true;
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new List<GenericItemOption>(ParseHelper.GetMediaWikiResultsFromHtml(SearchUrl, searchTerm, WebsiteUrl, LinkName));
    }
}