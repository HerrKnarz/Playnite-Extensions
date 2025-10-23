﻿using KNARZhelper;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.RAWG;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using Game = Playnite.SDK.Models.Game;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to RAWG.io.
    /// </summary>
    internal class LinkRawg : BaseClasses.Linker
    {
        public LinkRawg() => Settings.NeedsApiKey = true;
        public override string BaseUrl => "https://rawg.io/games/";
        public override string BrowserSearchUrl => "https://rawg.io/search?query=";
        public override string LinkName => "RAWG";
        public override string SearchUrl => "https://api.rawg.io/api/games?key={0}&search={1}&search_precise=true&page_size=50";

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
            if (string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                return base.GetSearchResults(searchTerm);
            }

            var rawgSearchResult = ApiHelper.GetJsonFromApi<RawgSearchResult>(string.Format(SearchUrl, Settings.ApiKey, searchTerm.UrlEncode()), LinkName);

            if (!rawgSearchResult?.Results?.Any() ?? true)
            {
                return base.GetSearchResults(searchTerm);
            }

            var searchResults = new List<GenericItemOption>();

            foreach (var result in rawgSearchResult.Results)
            {
                var genres = string.Empty;

                if (result.Genres?.Any() ?? false)
                {
                    genres = result.Genres.Select(genre => genre.Name)
                        .Aggregate((total, part) => total + ", " + part);
                }

                searchResults.Add(new SearchResult
                {
                    Name = result.Name,
                    Url = $"{BaseUrl}{result.Slug}",
                    Description = $"{result.Released}{Environment.NewLine}{genres}"
                });
            }

            return searchResults;
        }
    }
}