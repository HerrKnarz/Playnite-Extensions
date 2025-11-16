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
using System.Text;
using System.Threading.Tasks;

namespace ScreenshotUtilitiesSteamProvider
{
    public class ScreenshotUtilitiesSteamProvider : GenericPlugin, IScreenshotProviderPlugin
    {
        private readonly string _searchUrl = "https://steamcommunity.com/actions/SearchApps/";
        private Game _game;
        private ScreenshotGroup _screenshotGroup;

        public ScreenshotUtilitiesSteamProvider(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public override Guid Id { get; } = Guid.Parse("074c1cc0-a3ec-4ea2-a136-b6a01fbf0fae");
        public string Name { get; set; } = "Steam";
        public bool SupportsAutomaticScreenshots { get; set; } = true;
        public bool SupportsScreenshotSearch { get; set; } = true;

        public async Task<bool> CleanUpAsync(Game game) => await ScreenshotHelper.DeleteOrphanedJsonFiles(game.Id, Id);

        public async Task<bool> GetScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate) => await FetchScreenshotsAsync(game, daysSinceLastUpdate, forceUpdate);

        public string GetScreenshotSearchResult(Game game, string searchTerm)
        {
            var games = ApiHelper.GetJsonFromApi<List<SteamSearchResult>>($"{_searchUrl}{searchTerm.UrlEncode()}", Name, Encoding.UTF8);

            if (!(games?.Any() ?? false))
            {
                return null;
            }

            var result = new List<ScreenshotSearchResult>(games.Select(g => new ScreenshotSearchResult
            {
                Name = g.Name,
                Description = g.Appid,
                Identifier = g.Appid
            }));

            return Serialization.ToJson(result);
        }

        public async Task<bool> GetScreenshotsManualAsync(Game game, string gameIdentifier) => await FetchScreenshotsAsync(game, 0, true, gameIdentifier);

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                var notificationMessage = new NotificationMessage("Screenshot Utilities Steam Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);
            }
        }

        private async Task<bool> FetchScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate, string steamId = default)
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
                if (steamId != default && steamId.Equals(_screenshotGroup.GameIdentifier))
                {
                    return false;
                }

                // Get the right name to search for.
                var searchName = GetSteamId(steamId);

                if (string.IsNullOrEmpty(searchName))
                {
                    return false;
                }

                // We need to reset the file if we got a new steamId from the method call and it's
                // not the same we already got.
                if (!fileExists || (steamId != default && !searchName.Equals(_screenshotGroup.GameIdentifier)))
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

        private string GetSteamId(string steamId = default)
        {
            var searchSteamId = steamId;

            if (searchSteamId == default)
            {
                searchSteamId = _screenshotGroup.GameIdentifier;
            }

            if (searchSteamId == default)
            {
                searchSteamId = SteamHelper.GetSteamId(_game);
            }

            return searchSteamId;
        }

        private async Task<bool> LoadScreenshotsFromSourceAsync()
        {
            var apiUrl = $"https://store.steampowered.com/api/appdetails?appids={_screenshotGroup.GameIdentifier}";

            var result = await ApiHelper.GetJsonFromApiAsync<SteamAppDetails>(apiUrl, Name);

            var updated = false;

            if (!(result is null) && !(result[_screenshotGroup.GameIdentifier].Data.Screenshots is null) && (result[_screenshotGroup.GameIdentifier].Data.Screenshots?.Count) != 0)
            {
                _screenshotGroup.Screenshots
                    .AddRange(result[_screenshotGroup.GameIdentifier].Data.Screenshots
                    .Where(s => !_screenshotGroup.Screenshots.Any(es => es.Path.StripUriParams().Equals(s.PathFull.StripUriParams())))
                    .Select(s =>
                   new KNARZhelper.ScreenshotsCommon.Models.Screenshot(s.PathFull.StripUriParams())
                   {
                       ThumbnailPath = s.PathThumbnail.StripUriParams(),
                       SortOrder = s.Id
                   }));

                updated = true;
            }

            return updated;
        }
    }
}