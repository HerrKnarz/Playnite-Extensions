using HtmlAgilityPack;
using KNARZhelper;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using KNARZhelper.WebCommon;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ScreenshotUtilitiesArcadeDatabaseProvider
{
    public class ScreenshotUtilitiesArcadeDatabaseProvider : GenericPlugin, IScreenshotProviderPlugin
    {
        private const string _websiteUrl = "http://adb.arcadeitalia.net/";
        private readonly string _pageUrl = $"{_websiteUrl}dettaglio_mame.php?lang=en&game_name=";
        private readonly string _searchUrl = $"{_websiteUrl}lista_mame.php?lang=en&ricerca=";
        public override Guid Id { get; } = Guid.Parse("f2109af2-b240-4700-a61d-c316f47b8cf4");
        public bool SupportsAutomaticScreenshots { get; set; } = true;
        public bool SupportsScreenshotSearch { get; set; } = false;

        public ScreenshotUtilitiesArcadeDatabaseProvider(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public async Task<bool> GetScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate)
        {
            try
            {
                if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled || game == null)
                {
                    return false;
                }

                var romName = game.IsInstalled && (game.Roms?.Any() ?? false)
                ? Path.GetFileNameWithoutExtension(game.Roms[0].Path)
                : string.Empty;

                if (string.IsNullOrEmpty(romName))
                {
                    return false;
                }

                var fileName = ScreenshotHelper.GenerateFileName(game.Id, Id, Id);

                var screenshotGroup = ScreenshotGroup.CreateFromFile(new FileInfo(fileName))
                    ?? new ScreenshotGroup("Arcade Database", Id)
                    {
                        Provider = new ScreenshotProvider("Arcade Database", Id),
                        Screenshots = new RangeObservableCollection<Screenshot>()
                    };

                if (!forceUpdate
                    && screenshotGroup.LastUpdate != null
                    && (screenshotGroup.LastUpdate > DateTime.Now.AddDays(daysSinceLastUpdate * -1)))
                {
                    return false;
                }

                var url = $"{_pageUrl}{romName}";

                var updated = false;

                try
                {
                    var htmlWeb = new HtmlWeb
                    {
                        UseCookies = true,
                        BrowserTimeout = new TimeSpan(0, 0, 10),
                        UserAgent = WebHelper.AgentString
                    };

                    var document = await htmlWeb.LoadFromWebAsync(url);

                    if (htmlWeb.StatusCode != HttpStatusCode.OK || document == null)
                    {
                        return false;
                    }

                    var htmlNodes = document.DocumentNode.SelectNodes("//ul[@class='elenco_immagini']/li");

                    if (htmlNodes == null || (htmlNodes.Count == 0))
                    {
                        return false;
                    }

                    foreach (var node in htmlNodes.Where(n => n.SelectSingleNode("./div/img") != null))
                    {
                        var imageUrl = node.SelectSingleNode("./div/img").GetAttributeValue("data-custom-src_full", "");
                        var name = node.SelectSingleNode("./span").InnerText;

                        if (!screenshotGroup.Screenshots.Any(es => es.Path.Equals(imageUrl)))
                        {
                            screenshotGroup.Screenshots.Add(new Screenshot(imageUrl)
                            {
                                ThumbnailPath = imageUrl,
                                Name = name,
                                SortOrder = htmlNodes.IndexOf(node)
                            });
                        }
                    }

                    updated = true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }

                ScreenshotHelper.SaveScreenshotGroupJson(game, screenshotGroup);

                return updated;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error fetching screenshots for {game.Name}");
                return false;
            }
        }

        public Task<bool> GetScreenshotsManualAsync(Game game, ScreenshotSearchResult searchResult) => throw new NotImplementedException();

        public List<ScreenshotSearchResult> GetScreenshotSearchResult(Game game, string searchTerm)
        {
            try
            {
                var urlLoadResult = WebHelper.LoadHtmlDocument($"{_searchUrl}{searchTerm.UrlEncode()}");

                if (urlLoadResult.ErrorDetails.Length > 0 || urlLoadResult.Document is null)
                {
                    return null;
                }

                var htmlNodes = urlLoadResult.Document.DocumentNode.SelectNodes("//li[@class='elenco_galleria']");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<ScreenshotSearchResult>(htmlNodes.Select(n => new ScreenshotSearchResult()
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode("./a/div[@class='titolo_galleria']").InnerText),
                        Description =
                            $"{WebUtility.HtmlDecode(n.SelectSingleNode("./a/div[@class='romset_galleria']").InnerText)} - {WebUtility.HtmlDecode(n.SelectSingleNode("./a/div[@class='produttore_galleria']").InnerText)}",
                        Identifier = $"{_websiteUrl}{n.SelectSingleNode("./a").GetAttributeValue("href", "")}"
                    }));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data for game {game.Name}");
            }

            return null;
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                var notificationMessage = new NotificationMessage("Screenshot Utilities Arcade Database Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);
            }
        }
    }
}