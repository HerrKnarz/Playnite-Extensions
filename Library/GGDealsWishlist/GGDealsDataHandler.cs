using AngleSharp;
using AngleSharp.Dom;
using GGDealsWishlist.Models;
using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;

namespace GGDealsWishlist
{
    public class GGDealsDataHandler
    {
        private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        private readonly GGDealsWishlistSettings _settings;

        private readonly WebViewSettings _webViewSettings = new WebViewSettings
        {
            JavaScriptEnabled = true,
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
        };

        public GGDealsDataHandler(GGDealsWishlistSettings settings)
        {
            _settings = settings;
        }

        public GGDealsGames Games { get; } = new GGDealsGames();

        public void RetrieveGames(int page = 1)
        {
            if (string.IsNullOrEmpty(_settings.WishlistUrl))
            {
                return;
            }

            if (page <= 1)
            {
                Games.Clear();

                page = 1;
            }

            var document = LoadPage(ComposeUrl(page));

            if (document.StatusCode != System.Net.HttpStatusCode.OK || GameCountReached(document))
            {
                return;
            }

            GetGamesFromPage(document);

            if (IsLastPage(document))
            {
                return;
            }

            Thread.Sleep(200);

            RetrieveGames(page + 1);

            return;
        }

        private string ComposeUrl(int page)
        {
            if (page <= 1)
            {
                return _settings.WishlistUrl;
            }

            var uriBuilder = new UriBuilder(_settings.WishlistUrl);
            var paramValues = HttpUtility.ParseQueryString(uriBuilder.Query);
            paramValues.Add("page", page.ToString());
            uriBuilder.Query = paramValues.ToString();
            return uriBuilder.Uri.ToString();
        }

        private bool GameCountReached(IDocument document)
        {
            var gameCountString = document.QuerySelector("span.search-results-counter span:nth-of-type(2)")?.TextContent;
            int.TryParse(gameCountString?.Trim().FirstPart(" "), out var count);
            return count <= Games.Count;
        }

        private void GetGamesFromPage(IDocument document)
        {
            var cells = document.QuerySelectorAll("#wishlist-list div.wishlist-item");
            if (!cells.HasItems())
            {
                return;
            }

            foreach (var cell in cells)
            {
                var game = new GGDealsGame()
                {
                    Name = cell.Attributes["data-game-title"]?.Value,
                    GameId = cell.Attributes["data-container-game-id"]?.Value,
                    GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Type = GameActionType.URL,
                            Path = $"https://gg.deals/game/{cell.Attributes["data-game-name"]?.Value}",
                            IsPlayAction = true
                        }
                    },
                    Links = new List<Link>()
                    {
                        new Link()
                        {
                            Name = "GG.deals",
                            Url = $"https://gg.deals/game/{cell.Attributes["data-game-name"]?.Value}"
                        }
                    },
                    Source = new MetadataNameProperty("GG.deals Wishlist"),
                    IsInstalled = true
                };

                if (string.IsNullOrEmpty(game.Name) || string.IsNullOrEmpty(game.GameId))
                {
                    continue;
                }

                Games.Add(game);
            }
        }

        private bool IsLastPage(IDocument document)
        {
            var nextPage = document.QuerySelector("#wishlist-list-pagination-page-links li.selected + li");
            return nextPage == null || nextPage.Attributes["class"]?.Value?.Contains("disabled") == true;
        }

        private IDocument LoadPage(string url)
        {
            using (var webView = API.Instance.WebViews.CreateOffscreenView(_webViewSettings))
            {
                webView.NavigateAndWait(url);
                var htmlSource = webView.GetPageSource();
                webView.Close();

                return AsyncHelper.RunSync(async () => await _context.OpenAsync(req => req.Content(htmlSource)));
            }
        }
    }
}
