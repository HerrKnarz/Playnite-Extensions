using KNARZhelper;
using KNARZhelper.MetadataCommon;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using KNARZhelper.WebCommon;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ScreenshotUtilitiesKLOVProvider
{
    public class ScreenshotUtilitiesKLOVProvider : GenericPlugin, IScreenshotProviderPlugin
    {
        private const string _websiteName = "Killer List Of Video Games";
        private const string _websiteUrl = "https://www.arcade-museum.com/Videogame/";

        private Game _game;
        private ScreenshotGroup _screenshotGroup;

        public ScreenshotUtilitiesKLOVProvider(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public override Guid Id { get; } = Guid.Parse("10a18213-f522-4818-8e8d-2b063398b069");
        public string ProviderName { get; set; } = _websiteName;
        public bool SupportsAutomaticScreenshots { get; set; } = true;
        public bool SupportsScreenshotSearch { get; set; } = false;

        public async Task<bool> CleanUpAsync(Game game) => await ScreenshotHelper.DeleteOrphanedJsonFiles(game.Id, Id);

        public async Task<bool> FetchScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate)
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

                (fileExists, _screenshotGroup) = ScreenshotHelper.LoadGroup(game, _websiteName, Id);

                // return if we don't want to force an update and the last update was inside the
                // days configured.
                if (!forceUpdate
                    && _screenshotGroup.LastUpdate != null
                    && (_screenshotGroup.LastUpdate > DateTime.Now.AddDays(daysSinceLastUpdate * -1)))
                {
                    return false;
                }

                var link = MetadataHelper.GetLink(game, new System.Text.RegularExpressions.Regex(@"arcade-museum\.com\/(Videogame\/|game_detail.php\?game_id)"));

                if (link != null)
                {
                    _screenshotGroup.GameIdentifier = link.Url;
                }
                else
                {
                    // Get the right name to search for.
                    var searchName = game.Name
                        .RemoveDiacritics()
                        .RemoveSpecialChars()
                        .Replace("-", " ")
                        .CollapseWhitespaces()
                        .Replace(" ", "-")
                        .ToLower();

                    if (string.IsNullOrEmpty(searchName))
                    {
                        return false;
                    }

                    if (string.IsNullOrEmpty(_screenshotGroup.GameIdentifier))
                    {
                        _screenshotGroup.GameIdentifier = $"{_websiteUrl}{searchName}";
                    }
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

        public string GetScreenshotSearchResult(Game game, string searchTerm) => throw new NotImplementedException();

        public Task<bool> GetScreenshotsManualAsync(Game game, string gameIdentifier) => throw new NotImplementedException();

        public async Task<bool> HandleGameStoppedAsync(Game game) => false;

        public async Task<bool> LoadScreenshotsFromSourceAsync()
        {
            var url = _screenshotGroup.GameIdentifier;

            var updated = false;

            try
            {
                var urlLoadResult = await WebHelper.LoadHtmlDocumentAsync(url);

                if (urlLoadResult.StatusCode != HttpStatusCode.OK || urlLoadResult.Document == null)
                {
                    return false;
                }

                var gameName = WebUtility.HtmlDecode(urlLoadResult.Document.DocumentNode.SelectSingleNode("//h1")?.InnerText.Trim());

                var htmlNodes = urlLoadResult.Document.DocumentNode.SelectNodes("//a[@data-fancybox='gallery']");

                if (htmlNodes == null || (htmlNodes.Count == 0))
                {
                    return false;
                }

                foreach (var node in htmlNodes)
                {
                    var imageUrl = node.GetAttributeValue("data-src", "");

                    if (!imageUrl.StartsWith("http"))
                    {
                        continue;
                    }

                    var thumbNode = node.SelectSingleNode("./img") ?? node.SelectSingleNode("./figure/img");

                    var thumbNailUrl = string.Empty;

                    if (thumbNode != null)
                    {
                        thumbNailUrl = thumbNode?.GetAttributeValue("src", "");

                        if (string.IsNullOrEmpty(thumbNailUrl))
                        {
                            thumbNailUrl = thumbNode.GetAttributeValue("data-src", "");
                        }

                        if (string.IsNullOrEmpty(thumbNailUrl))
                        {
                            thumbNailUrl = imageUrl;
                        }
                    }

                    var name = WebUtility.HtmlDecode(node.SelectSingleNode("./figure/figcaption")?.InnerText.Trim()
                        ?? node.SelectSingleNode("./img")?.GetAttributeValue("alt", "")).CollapseWhitespaces();

                    name = name.Replace(gameName + " - ", "");

                    var mediaType = MediaType.Screenshot;

                    mediaType = name.Equals("Cabinet") ? MediaType.ArcadeCabinet
                        : name.Contains("Video Game Marquee") ? MediaType.ArcadeMarquee
                        : name.Contains("Video Game Screen") ? MediaType.Screenshot
                        : name.Contains("Game Logo") ? MediaType.Logo
                        : name.Contains("Cabinet - PCB") ? MediaType.ArcadeCircuit
                        : name.Contains("Cabinet - Control Panel") ? MediaType.ArcadeControlPanel
                        : name.Contains("Cabinet - Instruction Card") ? MediaType.ArcadeControlsInfo
                        : name.Contains("Cabinet - ") ? MediaType.ArcadeCabinet
                        : name.Equals("Flyer") ? MediaType.Advertisement
                        : name.Contains("Memorabilia") ? MediaType.Artwork
                        : MediaType.Unknown;

                    var screenshot = _screenshotGroup.Screenshots.FirstOrDefault(es => es.Path.Equals(imageUrl));

                    if (screenshot == default)
                    {
                        _screenshotGroup.Screenshots.Add(new Screenshot(imageUrl)
                        {
                            ThumbnailPath = thumbNailUrl,
                            Name = name,
                            SortOrder = htmlNodes.IndexOf(node),
                            Type = mediaType
                        });
                    }
                    else
                    {
                        screenshot.ThumbnailPath = thumbNailUrl;
                        screenshot.Name = name;
                        screenshot.SortOrder = htmlNodes.IndexOf(node);
                        screenshot.Type = mediaType;
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
                var notificationMessage = new NotificationMessage("Screenshot Utilities Killer List Of Video Games Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);
            }
        }
    }
}
