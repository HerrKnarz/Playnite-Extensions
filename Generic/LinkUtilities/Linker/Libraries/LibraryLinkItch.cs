using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Models;
using LinkUtilities.Models.Itch;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public static Guid Id { get; } = Guid.Parse("00000001-ebb2-4eec-abcb-7c89937a42bb");
        public override string LinkName { get; } = "Itch";
        public override LinkAddTypes AddType { get; } = LinkAddTypes.SingleSearchResult;
        public override string SearchUrl { get; } = "https://itch.io/api/1/{0}/search/games?query={1}";

        public override bool AddLibraryLink(Game game)
        {
            // To get the link to a game on the itch website, you need an API key and request the game data from their api.
            if (!string.IsNullOrWhiteSpace(Settings.ApiKey) && !LinkHelper.LinkExists(game, LinkName))
            {
                try
                {
                    string apiUrl = string.Format(_libraryUrl, Settings.ApiKey, game.GameId);

                    WebClient client = new WebClient();

                    string jsonResult = client.DownloadString(apiUrl);

                    ItchMetaData itchMetaData = JsonConvert.DeserializeObject<ItchMetaData>(jsonResult);

                    LinkUrl = itchMetaData.Game.Url;

                    return LinkHelper.AddLink(game, LinkName, LinkUrl);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error loading data from {LinkName}");

                    return false;
                }
            }

            return false;
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                try
                {
                    string apiUrl = string.Format(SearchUrl, Settings.ApiKey, searchTerm.UrlEncode());

                    WebClient client = new WebClient();

                    string jsonResult = client.DownloadString(apiUrl);

                    ItchSearchResult itchSearchResult = JsonConvert.DeserializeObject<ItchSearchResult>(jsonResult);

                    if (itchSearchResult.Games?.Any() ?? false)
                    {
                        foreach (SearchedGame game in itchSearchResult.Games)
                        {
                            SearchResults.Add(new SearchResult
                            {
                                Name = game.Title,
                                Url = game.Url,
                                Description = game.PublishedAt
                            }
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error loading data from {LinkName}");
                }
            }

            return base.SearchLink(searchTerm);
        }

        public LibraryLinkItch() : base() => Settings.NeedsApiKey = true;
    }
}
