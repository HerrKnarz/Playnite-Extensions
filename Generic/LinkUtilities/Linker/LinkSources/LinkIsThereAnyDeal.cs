using KNARZhelper;
using LinkUtilities.Models;
using LinkUtilities.Models.IsThereAnyDeal;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to IsThereAnyDeal.
    /// </summary>
    class LinkIsThereAnyDeal : Link
    {
        public override string LinkName { get; } = "IsThereAnyDeal";
        public override string BaseUrl => baseUrl;

        string baseUrl;

        string steamUrl { get; } = "https://isthereanydeal.com/steam/app/";
        string standardUrl { get; } = "https://isthereanydeal.com/game/";
        public override string SearchUrl { get; } = "https://api.isthereanydeal.com/v02/search/search/?key={0}&q={1}&limit=20&strict=0";

        public override string GetGamePath(Game game)
        {
            // IsThereAnyDeal provides links to steam games directly via the game id.
            if (game.PluginId == Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab"))
            {
                baseUrl = steamUrl;
                return game.GameId;
            }
            // For all other libraries links need the result name in lowercase without special characters and white spaces with numbers translated to roman numbers.
            else
            {
                baseUrl = standardUrl;
                return game.Name.RemoveDiacritics()
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

                    IsThereAnyDealSearchResult searchResult = Serialization.FromJson<IsThereAnyDealSearchResult>(jsonResult);

                    if (searchResult.Data.Results != null && searchResult.Data.Results.Count > 0)
                    {
                        int counter = 0;

                        foreach (Result result in searchResult.Data.Results)
                        {
                            counter++;

                            SearchResults.Add(new SearchResult
                            {
                                Name = $"{counter}. {result.Title}",
                                Url = $"{standardUrl}{result.Plain}",
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

        public LinkIsThereAnyDeal(LinkUtilities plugin) : base(plugin)
        {
            Settings.NeedsApiKey = true;
        }
    }
}
