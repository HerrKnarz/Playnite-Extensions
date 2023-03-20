using KNARZhelper;
using LinkUtilities.Models;
using LinkUtilities.Models.Epic;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace LinkUtilities.Linker
{
    class LinkEpic : Link
    {
        public override string LinkName { get; } = "Epic";
        public override string BaseUrl { get; } = "https://store.epicgames.com/en-US/p/";
        public override string SearchUrl { get; } = "https://www.epicgames.com/graphql?query={Catalog{searchStore(keywords:%22{SearchString}%22,category:%22games/edition%22,effectiveDate:%22[1900-01-01,{DateUntil}]%22,count:100){elements{title%20urlSlug%20seller{name}}}}}";

        private readonly string CheckUrl = "https://store-content-ipv4.ak.epicgames.com/api/en-US/content/products/";

        // Epic Links need the game name in lowercase without special characters and underscores instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public override bool AddLink(Game game)
        {
            // Unfortunately Epic returns the status code forbidden, when trying to check the url, because they want cookies and
            // javascipt active. Fortunately we can use the game slug in the store api. If it doesn't return an error, there should also
            // be a link with that slug.
            string gameSlug = GetGamePath(game);
            string url = $"{CheckUrl}{gameSlug}";

            WebClient client = new WebClient() { Encoding = Encoding.UTF8 };
            client.Headers.Add("Accept", "application/json");

            try
            {
                string _ = client.DownloadString(url);
                LinkUrl = $"{BaseUrl}{gameSlug}";
                return LinkHelper.AddLink(game, LinkName, LinkUrl, Plugin);
            }
            catch
            {
                LinkUrl = string.Empty;
                return false;
            }
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                WebClient client = new WebClient() { Encoding = Encoding.UTF8 };

                client.Headers.Add("Accept", "application/json");

                // We use replace instead of format, because the URL already contains several braces.
                string url = SearchUrl
                    .Replace("{SearchString}", searchTerm.UrlEncode())
                    .Replace("{DateUntil}", DateTime.Now.AddDays(5).ToString("yyyy-MM-dd"));

                string jsonResult = client.DownloadString(url);

                EpicSearchResult epicSearchResult = Serialization.FromJson<EpicSearchResult>(jsonResult);

                foreach (Element element in epicSearchResult.Data.Catalog.SearchStore.Elements)
                {
                    if (!string.IsNullOrEmpty(element.UrlSlug))
                    {
                        SearchResults.Add(new SearchResult
                        {
                            Name = element.Title,
                            Url = $"{BaseUrl}{element.UrlSlug}",
                            Description = element.Seller.Name
                        }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.SearchLink(searchTerm);
        }

        public LinkEpic(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
