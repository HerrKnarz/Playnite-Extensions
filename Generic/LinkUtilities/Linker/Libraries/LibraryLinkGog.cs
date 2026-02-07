using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Game = Playnite.SDK.Models.Game;

namespace LinkUtilities.Linker.Libraries
{
    /// <summary>
    /// Adds a link to GOG.
    /// </summary>
    internal class LibraryLinkGog : LibraryLink
    {
        public LibraryLinkGog() : base()
        {
            AllowedCallbackUrls.Add("https://www.gog.com/games");
        }

        public override bool AllowRedirects { get; set; } = false;
        public override string BaseUrl => "https://www.gog.com/en/game/";
        public override string BrowserSearchUrl => "https://www.gog.com/en/games?query=";

        /// <summary>
        /// ID of the game library to identify it in Playnite.
        /// </summary>
        public override Guid Id { get; } = LinkHelper.GogId;

        public override string LinkName => "GOG";
        public override bool ReturnsSameUrl { get; set; } = true;
        public override string SearchUrl => "https://catalog.gog.com/v1/catalog?limit=100&locale=en&order=desc:score&page=1&productType=in:game,pack&query=like:";

        public override bool FindLibraryLink(Game game, out List<Link> links)
        {
            links = new List<Link>();

            if (LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            var gogMetaData = ApiHelper.GetJsonFromApi<GogMetaData>($"https://api.gog.com/products/{game.GameId}", LinkName);

            if (!gogMetaData?.Slug?.Any() ?? true)
            {
                return false;
            }

            LinkUrl = $"{BaseUrl}{gogMetaData.Slug}";

            links.Add(new Link(LinkName, LinkUrl));

            return true;
        }

        // GOG Links need the game name in lowercase without special characters and underscores
        // instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace("-", "")
                .Replace(" ", "_")
                .ToLower();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            var gogSearchResult = ApiHelper.GetJsonFromApi<GogSearchResult>($"{SearchUrl}{searchTerm.RemoveDiacritics().UrlEncode()}", LinkName);

            var searchResults = new List<GenericItemOption>();

            if (!gogSearchResult?.Products?.Any() ?? true)
            {
                return searchResults;
            }

            foreach (var product in gogSearchResult.Products)
            {
                searchResults.Add(
                    new SearchResult
                    {
                        Name = product.Title,
                        Url = product.StoreLink,
                        Description = $"{product.ReleaseDate} -  ID {product.Id}"
                    });
            }

            return searchResults;
        }
    }
}