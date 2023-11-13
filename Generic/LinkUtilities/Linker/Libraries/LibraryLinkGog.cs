using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.Gog;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker
{
    /// <summary>
    ///     Adds a link to GOG.
    /// </summary>
    internal class LibraryLinkGog : LibraryLink
    {
        /// <summary>
        ///     ID of the game library to identify it in Playnite.
        /// </summary>
        public override Guid Id { get; } = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");

        public override string LinkName => "GOG";
        public override string BaseUrl => "https://www.gog.com/en/game/";
        public override string SearchUrl => "https://embed.gog.com/games/ajax/filtered?mediaType=game&search=";
        public override string BrowserSearchUrl => "https://www.gog.com/en/games?query=";

        public override bool AllowRedirects { get; set; } = false;

        public override bool ReturnsSameUrl { get; set; } = true;

        // GOG Links need the game name in lowercase without special characters and underscores instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace("-", "")
                .Replace(" ", "_")
                .ToLower();

        public override bool FindLibraryLink(Game game, out List<Link> links)
        {
            links = new List<Link>();

            if (LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            GogMetaData gogMetaData = ParseHelper.GetJsonFromApi<GogMetaData>($"https://api.gog.com/products/{game.GameId}", LinkName);

            if (!gogMetaData?.Slug?.Any() ?? true)
            {
                return false;
            }

            LinkUrl = $"{BaseUrl}{gogMetaData.Slug}";

            links.Add(new Link(LinkName, LinkUrl));

            return true;
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            GogSearchResult gogSearchResult = ParseHelper.GetJsonFromApi<GogSearchResult>($"{SearchUrl}{searchTerm.UrlEncode()}", LinkName);

            List<GenericItemOption> searchResults = new List<GenericItemOption>();

            if (!gogSearchResult?.Products?.Any() ?? true)
            {
                return searchResults;
            }

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

                searchResults.Add(
                    new SearchResult
                    {
                        Name = product.Title,
                        Url = $"{BaseUrl}{product.Slug}",
                        Description = releaseDate
                    });
            }

            return searchResults;
        }
    }
}