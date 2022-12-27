using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.Gog;
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
    class LibraryLinkGog : LibraryLink
    {
        public override Guid LibraryId { get; } = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");
        public override string LinkName { get; } = "GOG";
        public override string BaseUrl { get; } = "https://www.gog.com/en/game/";
        public override string SearchUrl { get; } = "https://embed.gog.com/games/ajax/filtered?mediaType=game&search=";
        public override bool AllowRedirects { get; set; } = false;
        public override string GetGamePath(Game game)
        {
            // GOG Links need the game name in lowercase without special characters and underscores instead of white spaces.
            return game.Name.RemoveDiacritics()
                .RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace("-", "")
                .Replace(" ", "_")
                .ToLower();
        }

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

                    GogMetaData gogMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<GogMetaData>(jsonResult);

                    LinkUrl = $"{BaseUrl}{gogMetaData.Slug}";

                    return LinkHelper.AddLink(game, LinkName, LinkUrl, Plugin);
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

            try
            {
                WebClient client = new WebClient();

                client.Headers.Add("Accept", "application/json");

                string jsonResult = client.DownloadString($"{SearchUrl}{searchTerm.UrlEncode()}");

                GogSearchResult gogSearchResult = Newtonsoft.Json.JsonConvert.DeserializeObject<GogSearchResult>(jsonResult);

                int counter = 0;

                foreach (Product product in gogSearchResult.Products)
                {
                    counter++;

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
                        Name = $"{counter}. {product.Title}",
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

        public LibraryLinkGog(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
