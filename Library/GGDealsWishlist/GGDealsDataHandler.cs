using AngleSharp;
using AngleSharp.Dom;
using GGDealsWishlist.Models;
using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace GGDealsWishlist
{
    public class GGDealsDataHandler
    {
        private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        private readonly Settings _settings;

        private readonly WebViewSettings _webViewSettings = new WebViewSettings
        {
            JavaScriptEnabled = true,
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
        };

        public GGDealsDataHandler(Settings settings)
        {
            _settings = settings;
        }

        public GGDealsGames Games { get; } = new GGDealsGames();

        public HashSet<string> ImportedGames => API.Instance.Database.Games.Where(g => g.PluginId == GGDealsWishlist.PluginId).Select(g => g.GameId).Distinct().ToHashSet();

        public void RetrieveGames(bool onlyNewGames = true, int page = 1)
        {
            //TODO: Change this to optionally only fetch new games up to the max count, so the addon doesn't have to process them all every time.

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

            if (document.StatusCode != System.Net.HttpStatusCode.OK || GameCountReached(document, onlyNewGames))
            {
                return;
            }

            GetGamesFromPage(document, onlyNewGames);

            if (onlyNewGames && Games.Count >= _settings.MaxGamesToImport)
            {
                return;
            }

            if (IsLastPage(document))
            {
                return;
            }

            Thread.Sleep(200);

            RetrieveGames(onlyNewGames, page + 1);

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

        private bool GameCountReached(IDocument document, bool onlyNewGames = true)
        {
            var maxCount = _settings.MaxGamesToImport;

            if (!onlyNewGames)
            {
                var gameCountString = document.QuerySelector("span.search-results-counter span:nth-of-type(2)")?.TextContent;

                if (!int.TryParse(gameCountString?.Trim().FirstPart(" "), out maxCount))
                {
                    return false;
                }
            }

            return maxCount <= Games.Count;
        }

        private void GetGamesFromPage(IDocument document, bool onlyNewGames = true)
        {
            var cells = document.QuerySelectorAll("#wishlist-list div.wishlist-item");
            if (!cells.HasItems())
            {
                return;
            }

            foreach (var cell in cells)
            {
                if (onlyNewGames && Games.Count >= _settings.MaxGamesToImport)
                {
                    return;
                }

                var platformHelper = new PlatformHelper(API.Instance.Database.Platforms);
                var platformString = cell.QuerySelector(".game-info-wrapper .platform-link-icon span")?.TextContent?.Trim();

                var gameId = cell.Attributes["data-container-game-id"]?.Value;

                if (string.IsNullOrEmpty(gameId) || (onlyNewGames && ImportedGames.Contains(gameId)))
                {
                    continue;
                }

                var game = new GGDealsGame()
                {
                    Name = cell.Attributes["data-game-title"]?.Value,
                    GameId = gameId,
                    Links = new List<Link>()
                    {
                        new Link()
                        {
                            Name = "GG.deals",
                            Url = $"https://gg.deals/game/{cell.Attributes["data-game-name"]?.Value}"
                        }
                    },
                    Platforms = platformHelper.GetPlatforms(platformString).ToHashSet(),
                    Source = new MetadataNameProperty("GG.deals Wishlist"),
                    IsInstalled = _settings.ImportGamesAsInstalled
                };

                if (!string.IsNullOrEmpty(_settings.DefaultCategory))
                {
                    game.Categories = new HashSet<MetadataProperty>()
                    {
                        new MetadataNameProperty(_settings.DefaultCategory)
                    };
                }

                if (_settings.ImportGamesAsInstalled)
                {
                    game.GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Type = GameActionType.URL,
                            Path = $"https://gg.deals/game/{cell.Attributes["data-game-name"]?.Value}",
                            IsPlayAction = true
                        }
                    };
                }

                if (string.IsNullOrEmpty(game.Name))
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
