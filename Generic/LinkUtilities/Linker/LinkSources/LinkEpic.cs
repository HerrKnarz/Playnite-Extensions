using KNARZhelper;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.Epic;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace LinkUtilities.Linker
{
    internal class LinkEpic : BaseClasses.Linker
    {
        public override string LinkName => "Epic";
        public override string BaseUrl => "https://store.epicgames.com/en-US/p/";
        public override string SearchUrl => "https://www.epicgames.com/graphql?query={Catalog{searchStore(keywords:%22{SearchString}%22,category:%22games/edition%22,effectiveDate:%22[1900-01-01,{DateUntil}]%22,count:100){elements{title%20urlSlug%20seller{name}}}}}";

        private readonly string _checkUrl = "https://store-content-ipv4.ak.epicgames.com/api/en-US/content/products/";

        // Epic Links need the game name in lowercase without special characters and underscores instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public override bool FindLinks(Game game, out List<Link> links)
        {
            links = new List<Link>();

            // Unfortunately Epic returns the status code forbidden, when trying to check the url, because they want cookies and
            // javascript active. Fortunately we can use the game slug in the store api. If it doesn't return an error, there should also
            // be a link with that slug.
            string gameSlug = GetGamePath(game);
            string url = $"{_checkUrl}{gameSlug}";

            WebClient client = new WebClient() { Encoding = Encoding.UTF8 };
            client.Headers.Add("Accept", "application/json");

            try
            {
                string _ = client.DownloadString(url);
                LinkUrl = $"{BaseUrl}{gameSlug}";

                links.Add(new Link(LinkName, LinkUrl));

                return true;
            }
            catch
            {
                LinkUrl = string.Empty;
                return false;
            }
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            // We use replace instead of format, because the URL already contains several braces.
            string url = SearchUrl
                .Replace("{SearchString}", searchTerm.UrlEncode())
                .Replace("{DateUntil}", DateTime.Now.AddDays(5).ToString("yyyy-MM-dd"));

            EpicSearchResult epicSearchResult = ParseHelper.GetJsonFromApi<EpicSearchResult>(url, LinkName, Encoding.UTF8);

            return epicSearchResult?.Data?.Catalog?.SearchStore?.Elements?.Any() ?? false
                ? new List<GenericItemOption>(epicSearchResult.Data.Catalog.SearchStore.Elements.Where(e => !string.IsNullOrEmpty(e.UrlSlug))
                    .Select(e => new SearchResult()
                    {
                        Name = e.Title,
                        Url = $"{BaseUrl}{e.UrlSlug}",
                        Description = e.Seller.Name
                    }))
                : base.GetSearchResults(searchTerm);
        }
    }
}