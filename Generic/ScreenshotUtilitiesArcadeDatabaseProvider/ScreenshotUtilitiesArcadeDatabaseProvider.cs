using HtmlAgilityPack;
using KNARZhelper;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using KNARZhelper.WebCommon;
using Playnite.SDK;
using Playnite.SDK.Data;
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
        public bool SupportsScreenshotSearch { get; set; } = true;

        public ScreenshotUtilitiesArcadeDatabaseProvider(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public async Task<bool> FetchScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate, string romName = default)
        {
            try
            {
                if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled || game == null)
                {
                    return false;
                }

                var searchRomName = romName == default
                    ? game.IsInstalled && (game.Roms?.Any() ?? false)
                        ? Path.GetFileNameWithoutExtension(game.Roms[0].Path)
                        : string.Empty
                    : romName;

                var fileName = ScreenshotHelper.GenerateFileName(game.Id, Id, Id);

                var screenshotGroup = ScreenshotGroup.CreateFromFile(new FileInfo(fileName))
                    ?? new ScreenshotGroup("Arcade Database", Id)
                    {
                        GameIdentifier = searchRomName,
                        Provider = new ScreenshotProvider("Arcade Database", Id),
                        Screenshots = new RangeObservableCollection<Screenshot>()
                    };

                if (!forceUpdate
                    && screenshotGroup.LastUpdate != null
                    && (screenshotGroup.LastUpdate > DateTime.Now.AddDays(daysSinceLastUpdate * -1)))
                {
                    return false;
                }

                if (romName != default && romName.Equals(screenshotGroup.GameIdentifier))
                {
                    return false;
                }

                // if we got a new romName from the method call we use that one going forward.
                if (romName != default && !romName.Equals(screenshotGroup.GameIdentifier))
                {
                    screenshotGroup.GameIdentifier = romName;

                    screenshotGroup.Screenshots.Clear();
                }
                else
                {
                    searchRomName = screenshotGroup.GameIdentifier;
                }

                if (string.IsNullOrEmpty(searchRomName))
                {
                    return false;
                }

                var url = $"{_pageUrl}{searchRomName}";

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

        public async Task<bool> GetScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate) => await FetchScreenshotsAsync(game, daysSinceLastUpdate, forceUpdate);

        public async Task<bool> GetScreenshotsManualAsync(Game game, string gameIdentifier) => await FetchScreenshotsAsync(game, 0, true, gameIdentifier);

        public string GetScreenshotSearchResult(Game game, string searchTerm)
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
                    var result = new List<ScreenshotSearchResult>(htmlNodes.Select(n => new ScreenshotSearchResult()
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode("./a/div[@class='titolo_galleria']").InnerText),
                        Description =
                            $"{WebUtility.HtmlDecode(n.SelectSingleNode("./a/div[@class='romset_galleria']").InnerText)} - {WebUtility.HtmlDecode(n.SelectSingleNode("./a/div[@class='produttore_galleria']").InnerText)}",
                        Identifier = WebUtility.HtmlDecode(n.SelectSingleNode("./a/div[@class='romset_galleria']").InnerText)
                    }));

                    return Serialization.ToJson(result);
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