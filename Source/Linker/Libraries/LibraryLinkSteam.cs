using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.Steam;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to the steam page of the game, if it is part of the steam library.
    /// </summary>
    class LibraryLinkSteam : LinkAndLibrary
    {
        public override Guid LibraryId { get; } = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
        public override string LinkName { get; } = "Steam";
        public override string SearchUrl { get; } = "https://steamcommunity.com/actions/SearchApps/";
        public string LibraryUrl { get; } = "https://store.steampowered.com/app/";

        public override bool AddLibraryLink(Game game)
        {
            // Adding a link to steam is extremely simple. You only have to add the GameId to the base URL.
            LinkUrl = $"{LibraryUrl}{game.GameId}";
            return LinkHelper.AddLink(game, LinkName, LinkUrl, Settings);
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                WebClient client = new WebClient();

                string jsonResult = client.DownloadString($"{SearchUrl}{searchTerm.UrlEncode()}");

                List<SteamSearchResult> games = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SteamSearchResult>>(jsonResult);

                if (games != null && games.Count > 0)
                {
                    int counter = 0;

                    foreach (SteamSearchResult game in games)
                    {
                        counter++;

                        SearchResults.Add(new SearchResult
                        {
                            Name = $"{counter}. {game.Name}",
                            Url = $"{LibraryUrl}{game.Appid}",
                            Description = game.Appid
                        }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }


            return base.SearchLink(searchTerm);
        }

        public LibraryLinkSteam(LinkUtilitiesSettings settings) : base(settings)
        {
        }
    }
}
