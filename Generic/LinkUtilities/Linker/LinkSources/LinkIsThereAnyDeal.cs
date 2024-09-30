using KNARZhelper;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using Game = Playnite.SDK.Models.Game;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to IsThereAnyDeal.
    /// </summary>
    internal class LinkIsThereAnyDeal : BaseClasses.Linker
    {
        private const string _standardUrl = "https://isthereanydeal.com/game/";
        private const string _steamUrl = "https://isthereanydeal.com/steam/app/";
        private string _baseUrl;

        public LinkIsThereAnyDeal() => Settings.NeedsApiKey = true;
        public override string BaseUrl => _baseUrl;
        public override string BrowserSearchUrl => "https://isthereanydeal.com/search/?q=";

        public override string LinkName => "IsThereAnyDeal";
        public override string SearchUrl => "https://api.isthereanydeal.com/games/search/v1?key={0}&title={1}";

        public override string GetGamePath(Game game, string gameName = null)
        {
            // IsThereAnyDeal provides links to steam games directly via the game id.
            if (game.PluginId == Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab"))
            {
                _baseUrl = _steamUrl;
                return game.GameId;
            }

            // For all other libraries links need the result name in lowercase without special characters and hyphens instead of white spaces.
            _baseUrl = _standardUrl;

            return (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("-", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower() + "/info/";
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                return base.GetSearchResults(searchTerm);
            }

            var searchResult = ParseHelper.GetJsonFromApi<IsThereAnyDealSearchResult[]>(string.Format(SearchUrl, Settings.ApiKey, searchTerm.UrlEncode()), LinkName);

            return searchResult?.Any(g => g.Type == "game") ?? false
                ? new List<GenericItemOption>(searchResult.Where(g => g.Type == "game").Select(r => new SearchResult
                {
                    Name = r.Title,
                    Url = $"{_standardUrl}{r.Slug}/info/",
                    Description = $"{r.Id}"
                }))
                : base.GetSearchResults(searchTerm);
        }
    }
}