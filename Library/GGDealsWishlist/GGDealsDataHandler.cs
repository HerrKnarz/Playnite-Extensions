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
    public static class AngleSharpHelper
    {
        public static string GetExclusiveText(this IElement node) => node.ChildNodes.OfType<IText>().Select(m => m.Text).FirstOrDefault();
    }

    public class GGDealsDataHandler
    {
        private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        private readonly Settings _settings;

        private readonly WebViewSettings _webViewSettings = new WebViewSettings
        {
            JavaScriptEnabled = true,
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
        };

        private PlatformHelper _platformHelper;

        public GGDealsDataHandler(Settings settings)
        {
            _settings = settings;
        }

        public GGDealsGames Games { get; } = new GGDealsGames();

        public Dictionary<string, Game> ImportedGames
        {
            get
            {
                var dict = new Dictionary<string, Game>();

                foreach (var game in API.Instance.Database.Games.Where(g => g.PluginId == GGDealsWishlist.PluginId))
                {
                    dict.Add(game.GameId, game);
                }

                return dict;
            }
        }

        public void RefreshGames()
        {
            API.Instance.MainView.UIDispatcher.Invoke(delegate
            {
                Games.Clear();
            });

            _platformHelper = new PlatformHelper(API.Instance.Database.Platforms);

            RetrieveGames();

            Games.LastRefresh = DateTime.Now;
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

        private bool GameCountReached(IDocument document)
        {
            var gameCountString = document.QuerySelector("span.search-results-counter span:nth-of-type(2)")?.TextContent;

            return int.TryParse(gameCountString?.Trim().FirstPart(" "), out var maxCount) && maxCount <= Games.Count;
        }

        private void GetGame(IElement cell)
        {
            var gameId = string.Empty;
            var gameName = string.Empty;

            try
            {
                gameId = cell.Attributes["data-container-game-id"]?.Value;
                gameName = cell.Attributes["data-game-title"]?.Value;

                Log.Debug(_settings.DebugMode, $"### GAME {gameId} - {gameName}: FETCHING DATA ########################################");

                if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(gameName))
                {
                    Log.Debug(_settings.DebugMode, $"### GAME {gameId} - {gameName}: ID OR NAME MISSING ########################################");
                    return;
                }

                ImportedGames.TryGetValue(gameId, out var existingGame);

                var linkUrl = $"https://gg.deals{cell.QuerySelector("a.full-link")?.Attributes["href"]?.Value}";

                var discountedPrice = cell.QuerySelector(".price-inner-wrapper .price")?.GetExclusiveText();

                var discountCodeValue = cell.QuerySelector(".code")?.Attributes["data-clipboard-text"]?.Value;

                var discountData = new DiscountData()
                {
                    Available = !cell.ClassList.Contains("Unavailable"),
                    DiscountString = cell.QuerySelector(".price-inner-wrapper .discount")?.GetExclusiveText(),
                    DiscountCode = cell.QuerySelector(".code")?.TextContent,
                    DiscountCodeValue = discountCodeValue,
                    DiscountedPriceString = discountedPrice,
                    HistoricalLow = cell.QuerySelector(".historical") != null,
                    RegularPriceString = cell.QuerySelector(".price-inner-wrapper .base-price")?.GetExclusiveText() ?? discountedPrice,
                    ShopLink = $"https://gg.deals{cell.QuerySelector("a.shop-link")?.Attributes["href"]?.Value}",
                    ShopName = cell.QuerySelector("img.shop-image-white")?.Attributes["alt"]?.Value,
                };

                var metadata = new GameMetadata()
                {
                    GameId = gameId,
                    Name = cell.Attributes["data-game-title"]?.Value,
                    Source = new MetadataNameProperty("GG.deals Wishlist"),
                    IsInstalled = _settings.ImportGamesAsInstalled,
                    Links = new List<Link>()
                    {
                        new Link()
                        {
                            Name = "GG.deals",
                            Url = linkUrl
                        }
                    },
                    Platforms = _platformHelper.GetPlatforms(cell.QuerySelector(".game-info-wrapper .platform-link-icon span")?.TextContent?.Trim()).ToHashSet(),
                };

                if (!string.IsNullOrEmpty(_settings.DefaultCategory))
                {
                    metadata.Categories = new HashSet<MetadataProperty>()
                    {
                        new MetadataNameProperty(_settings.DefaultCategory)
                    };
                }

                if (_settings.ImportGamesAsInstalled)
                {
                    metadata.GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Type = GameActionType.URL,
                            Path = linkUrl,
                            IsPlayAction = true
                        }
                    };
                }

                var game = new GGDealsGame(existingGame, metadata, discountData, _settings)
                {
                    GGDealsCoverLink = cell.QuerySelector("picture.game-picture img")?.Attributes["src"]?.Value
                };

                Log.Debug(_settings.DebugMode, $"### GAME {gameId} - {gameName}: FETCHED DATA. ########################################");

                Games.Add(game);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error fetching game: {gameId} - {gameName}");
            }
        }

        private void GetGamesFromPage(IDocument document)
        {
            try
            {
                var cells = document.QuerySelectorAll("#wishlist-list div.wishlist-item");
                if (!cells.HasItems())
                {
                    return;
                }

                Log.Debug(_settings.DebugMode, $"### {cells.Count()} GAMES FOUND ########################################");

                foreach (var cell in cells)
                {
                    GetGame(cell);
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

        private void RetrieveGames(int page = 1)
        {
            if (string.IsNullOrEmpty(_settings.WishlistUrl))
            {
                return;
            }

            if (page <= 1)
            {
                page = 1;
            }

            Log.Debug(_settings.DebugMode, $"### PAGE {page}: STARTED LOADING GAMES ########################################");

            var document = LoadPage(ComposeUrl(page));

            if (document.StatusCode != System.Net.HttpStatusCode.OK || GameCountReached(document))
            {
                return;
            }

            GetGamesFromPage(document);

            Log.Debug(_settings.DebugMode, $"### PAGE {page}: FINISHED LOADING GAMES ########################################");

            if (IsLastPage(document))
            {
                return;
            }

            Thread.Sleep(200);

            RetrieveGames(page + 1);

            return;
        }
    }
}
