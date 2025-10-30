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
using System.Windows.Controls;

namespace ScreenshotUtilitiesGOGProvider
{
    public class ScreenshotUtilitiesGOGProvider : GenericPlugin, IScreenshotProviderPlugin
    {
        private ScreenshotUtilitiesGOGProviderSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("228c70e8-7c89-46fc-b2c8-6e97966d01a4");
        public static Guid GogId = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");
        public static Guid GogOssId = Guid.Parse("03689811-3F33-4DFB-A121-2EE168FB9A5C");
        public bool SupportsAutomaticScreenshots { get; set; } = true;
        public bool SupportsScreenshotSearch { get; set; } = false;

        public ScreenshotUtilitiesGOGProvider(IPlayniteAPI api) : base(api)
        {
            settings = new ScreenshotUtilitiesGOGProviderSettingsViewModel(this);
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

                if (game.PluginId != GogId && game.PluginId != GogOssId)
                {
                    return false;
                }

                var gogId = game.GameId;

                if (string.IsNullOrEmpty(gogId))
                {
                    return false;
                }

                var fileName = ScreenshotHelper.GenerateFileName(game.Id, Id, Id);

                var screenshotGroup = ScreenshotGroup.CreateFromFile(new FileInfo(fileName))
                    ?? new ScreenshotGroup("GOG", Id)
                    {
                        Provider = new ScreenshotProvider("GOG", Id),
                        Screenshots = new RangeObservableCollection<KNARZhelper.ScreenshotsCommon.Models.Screenshot>()
                    };

                if (!forceUpdate
                    && screenshotGroup.LastUpdate != null
                    && (screenshotGroup.LastUpdate > DateTime.Now.AddDays(daysSinceLastUpdate * -1)))
                {
                    return false;
                }

                var apiUrl = $"https://api.gog.com/products/{gogId}?expand=screenshots";

                var result = await ApiHelper.GetJsonFromApiAsync<GogApiResult>(apiUrl, "GOG");

                var updated = false;

                if (!(result is null) && !(result.Screenshots is null) && (result.Screenshots?.Count) != 0)
                {
                    foreach (var screenshot in result.Screenshots.Where(s => !screenshotGroup.Screenshots.Any(es => es.Path.Contains(s.ImageId))))
                    {
                        var imageUrl = screenshot.FormattedImages.Where(fi => fi.FormatterName.Equals("ggvgl")).FirstOrDefault()?.ImageUrl ?? null;
                        var thumbUrl = screenshot.FormattedImages.Where(fi => fi.FormatterName.Equals("ggvgm")).FirstOrDefault()?.ImageUrl ?? null;

                        screenshotGroup.Screenshots.Add(new KNARZhelper.ScreenshotsCommon.Models.Screenshot(imageUrl)
                        {
                            ThumbnailPath = thumbUrl,
                            SortOrder = result.Screenshots.IndexOf(screenshot)
                        });
                    }

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
                var notificationMessage = new NotificationMessage("Screenshot Utilities GOG Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);
            }
        }

        public override ISettings GetSettings(bool firstRunSettings) => settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => null;//new ScreenshotUtilitiesGOGProviderSettingsView();
    }
}