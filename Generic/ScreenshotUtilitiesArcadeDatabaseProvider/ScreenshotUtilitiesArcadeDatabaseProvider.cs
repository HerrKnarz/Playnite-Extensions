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
        private const string _websiteName = "Arcade Database";
        private const string _websiteUrl = "http://adb.arcadeitalia.net/";
        private readonly string _pageUrl = $"{_websiteUrl}dettaglio_mame.php?lang=en&game_name=";
        private readonly string _searchUrl = $"{_websiteUrl}lista_mame.php?lang=en&ricerca=";

        private Game _game;
        private ScreenshotGroup _screenshotGroup;

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

        public string GetRomName(string romName = default)
        {
            var searchRomName = romName;

            if (searchRomName == default)
            {
                searchRomName = _screenshotGroup.GameIdentifier;
            }

            if (searchRomName == default)
            {
                searchRomName = _game.IsInstalled && (_game.Roms?.Any() ?? false)
                        ? Path.GetFileNameWithoutExtension(_game.Roms[0].Path)
                        : string.Empty;
            }

            return searchRomName;
        }

        public async Task<bool> CleanUpAsync(Game game) => await ScreenshotHelper.DeleteOrphanedJsonFiles(game.Id, Id);

        public async Task<bool> FetchScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate, string romName = default)
        {
            try
            {
                // return when the main addon isn't installed.
                if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled || game == null)
                {
                    return false;
                }

                // set game and load the file.
                _game = game;

                var fileExists = false;

                (fileExists, _screenshotGroup) = ScreenshotHelper.LoadGroups(game, _websiteName, Id);

                // return if we don't want to force an update and the last update was inside the days configured.
                if (!forceUpdate
                    && _screenshotGroup.LastUpdate != null
                    && (_screenshotGroup.LastUpdate > DateTime.Now.AddDays(daysSinceLastUpdate * -1)))
                {
                    return false;
                }

                // Return if a game was searched and it's the one we already have.
                if (romName != default && romName.Equals(_screenshotGroup.GameIdentifier))
                {
                    return false;
                }

                // Get the right name to search for.
                var searchName = GetRomName(romName);

                if (string.IsNullOrEmpty(searchName))
                {
                    return false;
                }

                // We need to reset the file if we got a new romName from the method call and it's not the same we already got.
                if (!fileExists || (romName != default && !searchName.Equals(_screenshotGroup.GameIdentifier)))
                {
                    _screenshotGroup.GameIdentifier = searchName;

                    _screenshotGroup.Screenshots.Clear();
                }

                var updated = await LoadScreenshotsFromSourceAsync();

                ScreenshotHelper.SaveScreenshotGroupJson(game, _screenshotGroup);

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

        public async Task<bool> LoadScreenshotsFromSourceAsync()
        {
            var url = $"{_pageUrl}{_screenshotGroup.GameIdentifier}";

            var updated = false;

            try
            {
                var urlLoadResult = await WebHelper.LoadHtmlDocumentAsync(url);

                if (urlLoadResult.StatusCode != HttpStatusCode.OK || urlLoadResult.Document == null)
                {
                    return false;
                }

                var htmlNodes = urlLoadResult.Document.DocumentNode.SelectNodes("//ul[@class='elenco_immagini']/li");

                if (htmlNodes == null || (htmlNodes.Count == 0))
                {
                    return false;
                }

                foreach (var node in htmlNodes.Where(n => n.SelectSingleNode("./div/img") != null))
                {
                    var imageUrl = node.SelectSingleNode("./div/img").GetAttributeValue("data-custom-src_full", "");
                    var name = node.SelectSingleNode("./span").InnerText;

                    if (!_screenshotGroup.Screenshots.Any(es => es.Path.Equals(imageUrl)))
                    {
                        _screenshotGroup.Screenshots.Add(new Screenshot(imageUrl)
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

            return updated;
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