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
                GogMetaData gogMetaData = ParseHelper.GetJsonFromApi<GogMetaData>($"https://api.gog.com/products/{game.GameId}", LinkName);

                if (gogMetaData?.Slug?.Any() ?? false)
                {
                    LinkUrl = $"{BaseUrl}{gogMetaData.Slug}";

                    return LinkHelper.AddLink(game, LinkName, LinkUrl);
                }
            }

            return false;
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            GogSearchResult gogSearchResult = ParseHelper.GetJsonFromApi<GogSearchResult>($"{SearchUrl}{searchTerm.UrlEncode()}", LinkName);

            List<GenericItemOption> searchResults = new List<GenericItemOption>();

            if (gogSearchResult?.Products?.Any() ?? false)
            {
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
            }

            return searchResults;
        }

        public LibraryLinkGog() : base()
        {
        }
    }
}
