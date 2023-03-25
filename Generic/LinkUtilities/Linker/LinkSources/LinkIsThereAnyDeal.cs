using KNARZhelper;
using LinkUtilities.Models;
using LinkUtilities.Models.IsThereAnyDeal;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to IsThereAnyDeal.
    /// </summary>
    internal class LinkIsThereAnyDeal : BaseClasses.Link
    {
        private string _baseUrl;
        private readonly string _steamUrl = "https://isthereanydeal.com/steam/app/";
        private readonly string _standardUrl = "https://isthereanydeal.com/game/";

        public override string LinkName { get; } = "IsThereAnyDeal";
        public override string BaseUrl => _baseUrl;
        public override string SearchUrl { get; } = "https://api.isthereanydeal.com/v02/search/search/?key={0}&q={1}&limit=20&strict=0";

        public override string GetGamePath(Game game, string gameName = null)
        {
            // IsThereAnyDeal provides links to steam games directly via the game id.
            if (game.PluginId == Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab"))
            {
                _baseUrl = _steamUrl;
                return game.GameId;
            }
            // For all other _libraries links need the result name in lowercase without special characters and white spaces with numbers translated to roman numbers.
            else
            {
                _baseUrl = _standardUrl;
                return (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("-", "")
                .Replace(" ", "")
                .DigitsToRomanNumbers()
                .ToLower();
            }
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

                    IsThereAnyDealSearchResult searchResult = JsonConvert.DeserializeObject<IsThereAnyDealSearchResult>(jsonResult);

                    if (searchResult.Data.Results?.Any() ?? false)
                    {
                        foreach (Result result in searchResult.Data.Results)
                        {
                            SearchResults.Add(new SearchResult
                            {
                                Name = result.Title,
                                Url = $"{_standardUrl}{result.Plain}",
                                Description = $"{result.Id}"
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

        public LinkIsThereAnyDeal() : base() => Settings.NeedsApiKey = true;
    }
}
