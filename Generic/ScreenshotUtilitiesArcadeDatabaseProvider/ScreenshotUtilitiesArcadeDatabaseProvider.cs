using HtmlAgilityPack;
using KNARZhelper;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
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
using System.Windows.Controls;

namespace ScreenshotUtilitiesArcadeDatabaseProvider
{
    public class ScreenshotUtilitiesArcadeDatabaseProvider : GenericPlugin, IScreenshotProviderPlugin
    {
        private ScreenshotUtilitiesArcadeDatabaseProviderSettingsViewModel settings { get; set; }
        public override Guid Id { get; } = Guid.Parse("f2109af2-b240-4700-a61d-c316f47b8cf4");
        public bool SupportsAutomaticScreenshots { get; set; } = true;
        public bool SupportsScreenshotSearch { get; set; } = false;

        public ScreenshotUtilitiesArcadeDatabaseProvider(IPlayniteAPI api) : base(api)
        {
            settings = new ScreenshotUtilitiesArcadeDatabaseProviderSettingsViewModel(this);
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

                var url = $"http://adb.arcadeitalia.net/dettaglio_mame.php?lang=en&game_name={romName}";

                var updated = false;

                try
                {
                    var htmlWeb = new HtmlWeb
                    {
                        UseCookies = true,
                        BrowserTimeout = new TimeSpan(0, 0, 10),
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
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

        public List<ScreenshotSearchResult> GetScreenshotSearchResult(Game game, string searchTerm) => throw new NotImplementedException();

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                var notificationMessage = new NotificationMessage("Screenshot Utilities Arcade Database Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);
            }
        }

        public override ISettings GetSettings(bool firstRunSettings) => settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => null;//new ScreenshotUtilitiesArcadeDatabaseProviderSettingsView();
    }
}