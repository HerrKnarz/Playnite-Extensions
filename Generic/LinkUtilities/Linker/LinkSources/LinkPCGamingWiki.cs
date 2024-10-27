using KNARZhelper;
using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to PCGamingWiki.
    /// </summary>
    internal class LinkPcGamingWiki : BaseClasses.Linker
    {
        private const string _websiteUrl = "https://www.pcgamingwiki.com";
        public override string BaseUrl => $"{_websiteUrl}/wiki/";
        public override string BrowserSearchUrl => $"{_websiteUrl}/w/index.php?search=";
        public override string LinkName => "PCGamingWiki";
        public override string SearchUrl => _websiteUrl + "/w/api.php?action=opensearch&format=xml&search={0}&limit=50";

        public override bool FindLinks(Game game, out List<Link> links)
        {
            LinkUrl = string.Empty;
            links = new List<Link>();

            if (LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            var gameName = GetGamePath(game, game.Name.RemoveEditionSuffix());

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
                var gameNameCapitalized = game.Name.CollapseWhitespaces().ToTitleCase().Replace(" ", "_").EscapeDataString();

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

        // PCGamingWiki Links need the game with underscores instead of whitespaces and special characters simply encoded.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).CollapseWhitespaces()
                .Replace(" ", "_")
                .EscapeDataString();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new List<GenericItemOption>(ParseHelper.GetMediaWikiResultsFromApi(SearchUrl, searchTerm, LinkName));
    }
}