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
            if (string.IsNullOrEmpty(_settings.WishlistUrl))
            {
                return;
            }

            if (page <= 1)
            {
                Games.Clear();

                page = 1;
            }

            Log.Debug(_settings.DebugMode, $"### PAGE {page}: STARTED LOADING GAMES ########################################");

            var document = LoadPage(ComposeUrl(page));

            if (document.StatusCode != System.Net.HttpStatusCode.OK || GameCountReached(document, onlyNewGames))
            {
                return;
            }

            GetGamesFromPage(document, onlyNewGames);

            Log.Debug(_settings.DebugMode, $"### PAGE {page}: FINISHED LOADING GAMES ########################################");

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
            if (page <= 1 && !_settings.OnlyImportGames)
            {
                Log.Debug(_settings.DebugMode, $"### LOADING URL: {_settings.WishlistUrl} ########################################");

                return _settings.WishlistUrl;
            }

            var uriBuilder = new UriBuilder(_settings.WishlistUrl);
            var paramValues = HttpUtility.ParseQueryString(uriBuilder.Query);

            if (page > 1)
            {
                paramValues.Set("page", page.ToString());
            }

            if (_settings.OnlyImportGames)
            {
                paramValues.Set("type", "1,3");
            }

            uriBuilder.Query = paramValues.ToString();

            var url = uriBuilder.Uri.ToString();

            Log.Debug(_settings.DebugMode, $"### LOADING URL: {url} ########################################");

            return url;
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
            try
            {
                var cells = document.QuerySelectorAll("#wishlist-list div.wishlist-item");
                if (!cells.HasItems())
                {
                    return;
                }

                Log.Debug(_settings.DebugMode, $"### {cells.Count()} GAMES FOUND ########################################");

                var platformHelper = new PlatformHelper(API.Instance.Database.Platforms);

                foreach (var cell in cells)
                {
                    var gameId = string.Empty;

                    try
                    {
                        if (onlyNewGames && Games.Count >= _settings.MaxGamesToImport)
                        {
                            return;
                        }

                        var platformString = cell.QuerySelector(".game-info-wrapper .platform-link-icon span")?.TextContent?.Trim();

                        gameId = cell.Attributes["data-container-game-id"]?.Value;

                        Log.Debug(_settings.DebugMode, $"### GAME {gameId}: FETCHING DATA ########################################");

                        if (string.IsNullOrEmpty(gameId) || (onlyNewGames && ImportedGames.Contains(gameId)))
                        {
                            Log.Debug(_settings.DebugMode, $"### GAME {gameId}: ALREADY IMPORTED ########################################");

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
                            Url = $"https://gg.deals{cell.QuerySelector("a.full-link")?.Attributes["href"]?.Value}"
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

                        Log.Debug(_settings.DebugMode, $"### GAME {gameId}: FETCHED DATA. GAME NAME: {game.Name} ########################################");

                        if (string.IsNullOrEmpty(game.Name))
                        {
                            continue;
                        }

                        Games.Add(game);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error fetching game with ID: {gameId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching games");
            }
        }

        private bool IsLastPage(IDocument document)
        {
            var nextPage = document.QuerySelector("#wishlist-list-pagination-page-links li.selected + li");
            var isLastPage = nextPage == null || nextPage.Attributes["class"]?.Value?.Contains("disabled") == true;

            Log.Debug(_settings.DebugMode, $"### LAST PAGED REACHED: {isLastPage} ########################################");

            return isLastPage;
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
