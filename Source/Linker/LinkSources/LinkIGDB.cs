using LinkUtilities.Helper;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to IGDB.
    /// </summary>
    class LinkIGDB : Link
    {
        public override string LinkName { get; } = "IGDB";
        public override string BaseUrl { get; } = "https://www.igdb.com/games/{0}";
        public override string SearchUrl { get; } = "https://api.igdb.com/v4/games";
        /// <summary>
        /// Checks if we are already authenticated.
        /// </summary>
        public bool IsAuthenticated { get => AuthenticatedUntil > DateTime.Now; }
        /// <summary>
        /// DateTime until the authentication lasts.
        /// </summary>
        public DateTime AuthenticatedUntil { get; set; } = DateTime.MinValue;
        /// <summary>
        /// Authentication token retrieved after authentication
        /// </summary>
        public IGDBAuthResponse AuthToken { get; set; }

        /// <summary>
        /// Authenticates Playnite with IGDB.
        /// </summary>
        /// <returns>True, if the authentication was successful.</returns>
        internal bool Authenticate()
        {
            if (!IsAuthenticated)
            {
                try
                {
                    string authUrl = $"https://id.twitch.tv/oauth2/token?client_id={Constants.IgdbClientId}&client_secret={Constants.IgdbClientSecret}&grant_type=client_credentials";

                    WebClient client = new WebClient();

                    client.Headers.Add("Accept", "application/json");

                    string jsonResult = client.UploadString(authUrl, "");

                    AuthToken = Newtonsoft.Json.JsonConvert.DeserializeObject<IGDBAuthResponse>(jsonResult);

                    // We substract ten seconds from the expire date to avoid overlaps.
                    AuthenticatedUntil = DateTime.Now.AddSeconds(AuthToken.ExpiresIn - 10);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error authenticating at IGDB");

                    return false;
                }
            }

            return IsAuthenticated;
        }

        public override bool AddLink(Game game)
        {
            if (!LinkHelper.LinkExists(game, LinkName))
            {
                // IGDB Links need the game name in lowercase with removed diacritics without special characters and hyphens
                // instead of white spaces.
                string gameName = game.Name.RemoveDiacritics().RemoveSpecialChars().CollapseWhitespaces().Replace(" ", "-").ToLower();

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

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            Authenticate();

            try
            {
                WebClient client = new WebClient();

                client.Headers.Add("Accept", "application/json");
                client.Headers.Add("Authorization", $"Bearer {AuthToken.AccessToken}");
                client.Headers.Add("Client-ID", Constants.IgdbClientId);

                string searchString = $"search \"{searchTerm.EscapeQuotes()}\"; fields name, url, release_dates.y, release_dates.platform.name;";

                string jsonResult = client.UploadString(SearchUrl, searchString);

                List<IGDBSearchResult> igdbSearchResults = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IGDBSearchResult>>(jsonResult);

                foreach (IGDBSearchResult result in igdbSearchResults)
                {
                    string description = string.Empty;
                    if (result.ReleaseDates != null && result.ReleaseDates.Count > 0)
                    {
                        description = result.ReleaseDates.Select(date => $"{date.Platform.Name} ({date.Y})").
                        Aggregate((total, part) => total + ", " + part);
                    }

                    SearchResults.Add(new SearchResult
                    {
                        Name = result.Name,
                        Url = result.Url,
                        Description = description
                    }
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading data from IGDB");
            }

            return base.SearchLink(searchTerm);
        }

        public LinkIGDB(LinkUtilitiesSettings settings) : base(settings)
        {
        }
    }
}