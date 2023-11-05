using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.Steam;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker
{
    /// <summary>
    ///     Adds a link to Steam.
    /// </summary>
    internal class LibraryLinkSteam : LibraryLink
    {
        private readonly string _libraryUrl = "https://store.steampowered.com/app/";

        /// <summary>
        ///     ID of the game library to identify it in Playnite.
        /// </summary>
        public override Guid Id { get; } = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");

        public override string LinkName => "Steam";
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string SearchUrl => "https://steamcommunity.com/actions/SearchApps/";

        public override bool FindLibraryLink(Game game, out List<Link> links)
        {
            links = new List<Link>();

            if (LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            // Adding a link to steam is extremely simple. You only have to add the GameId to the base URL.
            LinkUrl = $"{_libraryUrl}{game.GameId}";
            links.Add(new Link(LinkName, LinkUrl));

            return true;
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            List<SteamSearchResult> games = ParseHelper.GetJsonFromApi<List<SteamSearchResult>>($"{SearchUrl}{searchTerm.UrlEncode()}", LinkName);

            return games?.Any() ?? false
                ? new List<GenericItemOption>(games.Select(g => new SearchResult
                {
                    Name = g.Name,
                    Url = $"{_libraryUrl}{g.Appid}",
                    Description = g.Appid
                }))
                : base.GetSearchResults(searchTerm);
        }
    }
}