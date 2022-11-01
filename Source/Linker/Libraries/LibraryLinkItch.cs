using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.Itch;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Net;
using Game = Playnite.SDK.Models.Game;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to itch.io.
    /// </summary>
    class LibraryLinkItch : LinkAndLibrary
    {
        public override Guid LibraryId { get; } = Guid.Parse("00000001-ebb2-4eec-abcb-7c89937a42bb");
        public override string LinkName { get; } = "Itch";
        public override string SearchUrl { get; } = "https://itch.io/api/1/{0}/search/games?query={1}";
        public string LibraryUrl { get; } = "https://itch.io/api/1/{0}/game/{1}";

        public override bool AddLibraryLink(Game game)
        {
            // To get the link to a game on the itch website, you need an API key and request the game data from their api.
            if (!string.IsNullOrWhiteSpace(Plugin.Settings.Settings.ItchApiKey) && !LinkHelper.LinkExists(game, LinkName))
            {
                try
                {
                    string apiUrl = string.Format(LibraryUrl, Plugin.Settings.Settings.ItchApiKey, game.GameId);

                    WebClient client = new WebClient();

                    string jsonResult = client.DownloadString(apiUrl);

                    ItchMetaData itchMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<ItchMetaData>(jsonResult);

                    LinkUrl = itchMetaData.Game.Url;

                    return LinkHelper.AddLink(game, LinkName, LinkUrl, Plugin.Settings.Settings);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error loading data from {LinkName}");

                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            if (!string.IsNullOrWhiteSpace(Plugin.Settings.Settings.ItchApiKey))
            {
                try
                {
                    string apiUrl = string.Format(SearchUrl, Plugin.Settings.Settings.ItchApiKey, searchTerm.UrlEncode());

                    WebClient client = new WebClient();

                    string jsonResult = client.DownloadString(apiUrl);

                    ItchSearchResult itchSearchResult = Newtonsoft.Json.JsonConvert.DeserializeObject<ItchSearchResult>(jsonResult);

                    if (itchSearchResult.Games != null && itchSearchResult.Games.Count > 0)
                    {
                        int counter = 0;

                        foreach (SearchedGame game in itchSearchResult.Games)
                        {
                            counter++;

                            SearchResults.Add(new SearchResult
                            {
                                Name = $"{counter}. {game.Title}",
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

        public LibraryLinkItch(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
