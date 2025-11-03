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
using System.Threading.Tasks;

namespace ScreenshotUtilitiesSteamProvider
{
    public class ScreenshotUtilitiesSteamProvider : GenericPlugin, IScreenshotProviderPlugin
    {
        public override Guid Id { get; } = Guid.Parse("074c1cc0-a3ec-4ea2-a136-b6a01fbf0fae");
        public string Name { get; set; } = "Steam";
        public bool SupportsAutomaticScreenshots { get; set; } = true;
        public bool SupportsScreenshotSearch { get; set; } = false;

        public ScreenshotUtilitiesSteamProvider(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public async Task<bool> CleanUpAsync(Game game) => await ScreenshotHelper.DeleteOrphanedJsonFiles(game.Id, Id);

        public async Task<bool> GetScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate)
        {
            try
            {
                if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled || game == null)
                {
                    return false;
                }

                var steamId = SteamHelper.GetSteamId(game);

                if (string.IsNullOrEmpty(steamId))
                {
                    return false;
                }

                var fileName = ScreenshotHelper.GenerateFileName(game.Id, Id, Id);

                var screenshotGroup = ScreenshotGroup.CreateFromFile(new FileInfo(fileName))
                    ?? new ScreenshotGroup(Name, Id)
                    {
                        Provider = new ScreenshotProvider(Name, Id),
                        Screenshots = new RangeObservableCollection<KNARZhelper.ScreenshotsCommon.Models.Screenshot>()
                    };

                if (!forceUpdate
                    && screenshotGroup.LastUpdate != null
                    && (screenshotGroup.LastUpdate > DateTime.Now.AddDays(daysSinceLastUpdate * -1)))
                {
                    return false;
                }

                var apiUrl = $"https://store.steampowered.com/api/appdetails?appids={steamId}";

                var result = await ApiHelper.GetJsonFromApiAsync<SteamAppDetails>(apiUrl, Name);

                var updated = false;

                if (!(result is null) && !(result[steamId].Data.Screenshots is null) && (result[steamId].Data.Screenshots?.Count) != 0)
                {
                    screenshotGroup.Screenshots
                        .AddRange(result[steamId].Data.Screenshots
                        .Where(s => !screenshotGroup.Screenshots.Any(es => es.Path.StripUriParams().Equals(s.PathFull.StripUriParams())))
                        .Select(s =>
                       new KNARZhelper.ScreenshotsCommon.Models.Screenshot(s.PathFull.StripUriParams())
                       {
                           ThumbnailPath = s.PathThumbnail.StripUriParams(),
                           SortOrder = s.Id
                       }));

                    updated = true;
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

        public Task<bool> GetScreenshotsManualAsync(Game game, string gameIdentifier) => throw new NotImplementedException();

        public string GetScreenshotSearchResult(Game game, string searchTerm) => throw new NotImplementedException();

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                var notificationMessage = new NotificationMessage("Screenshot Utilities Steam Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);
            }
        }
    }
}