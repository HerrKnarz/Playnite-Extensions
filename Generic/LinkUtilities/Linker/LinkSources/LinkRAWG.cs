using KNARZhelper;
using LinkUtilities.Models;
using LinkUtilities.Models.RAWG;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to RAWG.io.
    /// </summary>
    class LinkRAWG : Link
    {
        public override string LinkName { get; } = "RAWG";
        public override string BaseUrl { get; } = "https://rawg.io/games/";
        public override string SearchUrl { get; } = "https://api.rawg.io/api/games?key={0}&search={1}&search_precise=true&page_size=50";

        public override string GetGamePath(Game game, string gameName = null)
        {
            // RAWG Links need the result name in lowercase without special characters and hyphens instead of white spaces.
            return (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("-", "")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                try
                {
                    string apiUrl = string.Format(SearchUrl, Settings.ApiKey, searchTerm.UrlEncode());

                    WebClient client = new WebClient();

                    string jsonResult = client.DownloadString(apiUrl);

                    RawgSearchResult rawgSearchResult = Serialization.FromJson<RawgSearchResult>(jsonResult);

                    if (rawgSearchResult.Results != null && rawgSearchResult.Results.Count > 0)
                    {
                        foreach (Result result in rawgSearchResult.Results)
                        {
                            string genres = string.Empty;
                            if (result.Genres != null && result.Genres.Count > 0)
                            {
                                genres = result.Genres.Select(genre => genre.Name).
                                Aggregate((total, part) => total + ", " + part);
                            }

                            SearchResults.Add(new SearchResult
                            {
                                Name = result.Name,
                                Url = $"{BaseUrl}{result.Slug}",
                                Description = $"{result.Released}{Environment.NewLine}{genres}"
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

        public LinkRAWG(LinkUtilities plugin) : base(plugin)
        {
            Settings.NeedsApiKey = true;
        }
    }
}
