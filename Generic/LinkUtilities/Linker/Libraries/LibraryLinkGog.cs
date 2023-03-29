using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Models;
using LinkUtilities.Models.Gog;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to GOG.
    /// </summary>
    internal class LibraryLinkGog : LibraryLink
    {
        /// <summary>
        /// ID of the game library to identify it in Playnite.
        /// </summary>
        public override Guid Id { get; } = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");
        public override string LinkName { get; } = "GOG";
        public override string BaseUrl { get; } = "https://www.gog.com/en/game/";
        public override string SearchUrl { get; } = "https://embed.gog.com/games/ajax/filtered?mediaType=game&search=";
        public override bool AllowRedirects { get; set; } = false;
        // GOG Links need the game name in lowercase without special characters and underscores instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace("-", "")
                .Replace(" ", "_")
                .ToLower();

        public override bool AddLibraryLink(Game game)
        {
            if (!LinkHelper.LinkExists(game, LinkName))
            {
                try
                {
                    // To add a link to the gog page you have to get it from their API via the GameId.
                    WebClient client = new WebClient();

                    client.Headers.Add("Accept", "application/json");

                    string jsonResult = client.DownloadString("https://api.gog.com/products/" + game.GameId);

                    GogMetaData gogMetaData = JsonConvert.DeserializeObject<GogMetaData>(jsonResult);

                    LinkUrl = $"{BaseUrl}{gogMetaData.Slug}";

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

            try
            {
                WebClient client = new WebClient();

                client.Headers.Add("Accept", "application/json");

                string jsonResult = client.DownloadString($"{SearchUrl}{searchTerm.UrlEncode()}");

                GogSearchResult gogSearchResult = JsonConvert.DeserializeObject<GogSearchResult>(jsonResult);

                foreach (Product product in gogSearchResult.Products)
                {
                    string releaseDate = string.Empty;

                    if (product.GlobalReleaseDate.HasValue)
                    {
                        releaseDate = MiscHelper.UnixTimeStampToDateTime(product.GlobalReleaseDate.Value).Date.ToString();
                    }
                    else if (product.ReleaseDate.HasValue)
                    {
                        releaseDate = MiscHelper.UnixTimeStampToDateTime(product.ReleaseDate.Value).Date.ToString();
                    }

                    SearchResults.Add(new SearchResult
                    {
                        Name = product.Title,
                        Url = $"{BaseUrl}{product.Slug}",
                        Description = releaseDate
                    }
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.SearchLink(searchTerm);
        }

        public LibraryLinkGog() : base()
        {
        }
    }
}
