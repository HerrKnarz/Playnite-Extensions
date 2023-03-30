using KNARZhelper;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.RAWG;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to RAWG.io.
    /// </summary>
    internal class LinkRAWG : BaseClasses.Linker
    {
        public override string LinkName { get; } = "RAWG";
        public override string BaseUrl { get; } = "https://rawg.io/games/";
        public override string SearchUrl { get; } = "https://api.rawg.io/api/games?key={0}&search={1}&search_precise=true&page_size=50";

        // RAWG Links need the result name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
           => (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("-", "")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                RawgSearchResult rawgSearchResult = ParseHelper.GetJsonFromApi<RawgSearchResult>(string.Format(SearchUrl, Settings.ApiKey, searchTerm.UrlEncode()), LinkName);

                if (rawgSearchResult?.Results?.Any() ?? false)
                {
                    List<GenericItemOption> searchResults = new List<GenericItemOption>();

                    foreach (Result result in rawgSearchResult.Results)
                    {
                        string genres = result.Genres?.Select(genre => genre.Name).Aggregate((total, part) => total + ", " + part) ?? string.Empty;

                        searchResults.Add(new SearchResult
                        {
                            Name = result.Name,
                            Url = $"{BaseUrl}{result.Slug}",
                            Description = $"{result.Released}{Environment.NewLine}{genres}"
                        }
                        );
                    }

                    return searchResults;
                }
            }

            return base.GetSearchResults(searchTerm);
        }

        public LinkRAWG() : base() => Settings.NeedsApiKey = true;
    }
}
