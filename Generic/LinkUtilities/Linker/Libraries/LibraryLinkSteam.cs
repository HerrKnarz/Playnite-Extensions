using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Models;
using LinkUtilities.Models.Steam;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Steam.
    /// </summary>
    internal class LibraryLinkSteam : LibraryLink
    {
        private readonly string _libraryUrl = "https://store.steampowered.com/app/";

        public override Guid LibraryId { get; } = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
        public override string LinkName { get; } = "Steam";
        public override LinkAddTypes AddType { get; } = LinkAddTypes.SingleSearchResult;
        public override string SearchUrl { get; } = "https://steamcommunity.com/actions/SearchApps/";

        public override bool AddLibraryLink(Game game)
        {
            // Adding a link to steam is extremely simple. You only have to add the GameId to the base URL.
            LinkUrl = $"{_libraryUrl}{game.GameId}";
            return LinkHelper.AddLink(game, LinkName, LinkUrl, Plugin);
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                WebClient client = new WebClient();

                string url = $"{SearchUrl}{searchTerm.UrlEncode()}";

                Log.Debug(url);

                string jsonResult = client.DownloadString(url);

                Log.Debug(jsonResult);

                List<SteamSearchResult> games = JsonConvert.DeserializeObject<List<SteamSearchResult>>(jsonResult);

                if (games != null && games.Count > 0)
                {
                    Log.Debug($"Games found: {games.Count}");

                    foreach (SteamSearchResult game in games)
                    {
                        SearchResults.Add(new SearchResult
                        {
                            Name = game.Name,
                            Url = $"{_libraryUrl}{game.Appid}",
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

        public LibraryLinkSteam(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
