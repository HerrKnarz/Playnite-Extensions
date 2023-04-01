using KNARZhelper;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.IsThereAnyDeal;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to IsThereAnyDeal.
    /// </summary>
    internal class LinkIsThereAnyDeal : BaseClasses.Linker
    {
        private string _baseUrl;
        private readonly string _steamUrl = "https://isthereanydeal.com/steam/app/";
        private readonly string _standardUrl = "https://isthereanydeal.com/game/";

        public override string LinkName => "IsThereAnyDeal";
        public override string BaseUrl => _baseUrl;
        public override string SearchUrl => "https://api.isthereanydeal.com/v02/search/search/?key={0}&q={1}&limit=20&strict=0";

        public override string GetGamePath(Game game, string gameName = null)
        {
            // IsThereAnyDeal provides links to steam games directly via the game id.
            if (game.PluginId == Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab"))
            {
                _baseUrl = _steamUrl;
                return game.GameId;
            }

            // For all other libraries links need the result name in lowercase without special characters and white spaces with numbers translated to roman numbers.
            _baseUrl = _standardUrl;

            return (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("-", "")
                .Replace(" ", "")
                .DigitsToRomanNumbers()
                .ToLower();
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                return base.GetSearchResults(searchTerm);
            }

            IsThereAnyDealSearchResult searchResult = ParseHelper.GetJsonFromApi<IsThereAnyDealSearchResult>(string.Format(SearchUrl, Settings.ApiKey, searchTerm.UrlEncode()), LinkName);

            return searchResult?.Data?.Results?.Any() ?? false
                ? new List<GenericItemOption>(searchResult.Data.Results.Select(r => new SearchResult()
                {
                    Name = r.Title,
                    Url = $"{_standardUrl}{r.Plain}",
                    Description = $"{r.Id}"
                }))
                : base.GetSearchResults(searchTerm);
        }

        public LinkIsThereAnyDeal() => Settings.NeedsApiKey = true;
    }
}