using KNARZhelper;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Controls;

namespace ScreenshotUtilitiesGOGProvider
{
    public class ScreenshotUtilitiesGOGProvider : GenericPlugin, IScreenshotProviderPlugin
    {
        public static Guid GogId = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");
        public static Guid GogOssId = Guid.Parse("03689811-3F33-4DFB-A121-2EE168FB9A5C");

        private readonly string _searchUrl = "https://catalog.gog.com/v1/catalog?limit=100&locale=en&order=desc:score&page=1&productType=in:game,pack&query=like:";
        private Game _game;
        private ScreenshotGroup _screenshotGroup;

        public ScreenshotUtilitiesGOGProvider(IPlayniteAPI api) : base(api)
        {
            settings = new ScreenshotUtilitiesGOGProviderSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public override Guid Id { get; } = Guid.Parse("228c70e8-7c89-46fc-b2c8-6e97966d01a4");
        public string Name { get; set; } = "GOG";
        public bool SupportsAutomaticScreenshots { get; set; } = true;
        public bool SupportsScreenshotSearch { get; set; } = true;
        private ScreenshotUtilitiesGOGProviderSettingsViewModel settings { get; set; }

        public async Task<bool> CleanUpAsync(Game game) => await ScreenshotHelper.DeleteOrphanedJsonFiles(game.Id, Id);

        public async Task<bool> GetScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate)
        {
            return await FetchScreenshotsAsync(game, daysSinceLastUpdate, forceUpdate);
        }

        public string GetScreenshotSearchResult(Game game, string searchTerm)
        {
            var gogSearchResult = ApiHelper.GetJsonFromApi<GogSearchResult>($"{_searchUrl}{searchTerm.RemoveDiacritics().UrlEncode()}", Name);

            var searchResults = new List<GenericItemOption>();

            if (!gogSearchResult?.Products?.Any() ?? true)
            {
                return null;
            }

            var result = new List<ScreenshotSearchResult>(gogSearchResult.Products.Select(product => new ScreenshotSearchResult
            {
                Name = product.Title,
                Description = $"{product.ReleaseDate} -  ID {product.Id}",
                Identifier = product.Id
            }));

            return Serialization.ToJson(result);
        }

        public async Task<bool> GetScreenshotsManualAsync(Game game, string gameIdentifier)
        {
            return await FetchScreenshotsAsync(game, 0, true, gameIdentifier);
        }

        public override ISettings GetSettings(bool firstRunSettings) => settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => null;

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                var notificationMessage = new NotificationMessage("Screenshot Utilities GOG Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);
            }
        }

        //new ScreenshotUtilitiesGOGProviderSettingsView();

        private async Task<bool> FetchScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate, string gogId = default)
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

                (fileExists, _screenshotGroup) = ScreenshotHelper.LoadGroup(_game, Name, Id);

                // return if we don't want to force an update and the last update was inside the
                // days configured.
                if (!forceUpdate
                    && _screenshotGroup.LastUpdate != null
                    && (_screenshotGroup.LastUpdate > DateTime.Now.AddDays(daysSinceLastUpdate * -1)))
                {
                    return false;
                }

                // Return if a game was searched and it's the one we already have.
                if (gogId != default && gogId.Equals(_screenshotGroup.GameIdentifier))
                {
                    return false;
                }

                // Get the right name to search for.
                var searchName = GetGogId(gogId);

                if (string.IsNullOrEmpty(searchName))
                {
                    return false;
                }

                // We need to reset the file if we got a new gogId from the method call and it's not
                // the same we already got.
                if (!fileExists || (gogId != default && !searchName.Equals(_screenshotGroup.GameIdentifier)))
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

        private string GetGogId(string gogId = default)
        {
            var searchGogId = gogId;

            if (searchGogId == default)
            {
                searchGogId = _screenshotGroup.GameIdentifier;
            }

            if ((searchGogId == default) && _game.PluginId.IsOneOf(GogId, GogOssId))
            {
                searchGogId = _game.GameId;
            }

            return searchGogId;
        }

        private async Task<bool> LoadScreenshotsFromSourceAsync()
        {
            var apiUrl = $"https://api.gog.com/products/{_screenshotGroup.GameIdentifier}?expand=screenshots";

            var result = await ApiHelper.GetJsonFromApiAsync<GogApiResult>(apiUrl, Name);

            var updated = false;

            if (!(result is null) && !(result.Screenshots is null) && (result.Screenshots?.Count) != 0)
            {
                foreach (var screenshot in result.Screenshots.Where(s => !_screenshotGroup.Screenshots.Any(es => es.Path.Contains(s.ImageId))))
                {
                    var imageUrl = screenshot.FormattedImages.Where(fi => fi.FormatterName.Equals("ggvgl")).FirstOrDefault()?.ImageUrl ?? null;
                    var thumbUrl = screenshot.FormattedImages.Where(fi => fi.FormatterName.Equals("ggvgm")).FirstOrDefault()?.ImageUrl ?? null;

                    _screenshotGroup.Screenshots.Add(new KNARZhelper.ScreenshotsCommon.Models.Screenshot(imageUrl)
                    {
                        ThumbnailPath = thumbUrl,
                        SortOrder = result.Screenshots.IndexOf(screenshot)
                    });
                }

                updated = true;
            }

            return updated;
        }
    }
}