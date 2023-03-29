using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.Itch;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using Game = Playnite.SDK.Models.Game;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to itch.io.
    /// </summary>
    internal class LibraryLinkItch : LibraryLink
    {
        private readonly string _libraryUrl = "https://itch.io/api/1/{0}/game/{1}";

        /// <summary>
        /// ID of the game library to identify it in Playnite.
        /// </summary>
        public override Guid Id { get; } = Guid.Parse("00000001-ebb2-4eec-abcb-7c89937a42bb");
        public override string LinkName { get; } = "Itch";
        public override LinkAddTypes AddType { get; } = LinkAddTypes.SingleSearchResult;
        public override string SearchUrl { get; } = "https://itch.io/api/1/{0}/search/games?query={1}";

        public override bool AddLibraryLink(Game game)
        {
            // To get the link to a game on the itch website, you need an API key and request the game data from their api.
            if (!string.IsNullOrWhiteSpace(Settings.ApiKey) && !LinkHelper.LinkExists(game, LinkName))
            {
                ItchMetaData itchMetaData = ParseHelper.GetJsonFromApi<ItchMetaData>(string.Format(_libraryUrl, Settings.ApiKey, game.GameId), LinkName);

                LinkUrl = itchMetaData?.Game?.Url ?? string.Empty;

                if (LinkUrl.Any())
                {
                    return LinkHelper.AddLink(game, LinkName, LinkUrl);
                }
            }

            return false;
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                ItchSearchResult itchSearchResult = ParseHelper.GetJsonFromApi<ItchSearchResult>(string.Format(SearchUrl, Settings.ApiKey, searchTerm.UrlEncode()), LinkName);

                if (itchSearchResult?.Games?.Any() ?? false)
                {
                    return new List<GenericItemOption>(itchSearchResult.Games.Select(g => new SearchResult()
                    {
                        Name = g.Title,
                        Url = g.Url,
                        Description = g.PublishedAt
                    }));
                }
            }

            return base.GetSearchResults(searchTerm);
        }

        public LibraryLinkItch() : base() => Settings.NeedsApiKey = true;
    }
}
