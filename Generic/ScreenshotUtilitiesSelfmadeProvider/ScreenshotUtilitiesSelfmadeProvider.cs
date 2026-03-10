using KNARZhelper;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ScreenshotUtilitiesSelfmadeProvider
{
    public class ScreenshotUtilitiesSelfmadeProvider : GenericPlugin, IScreenshotProviderPlugin
    {
        private readonly string _placeholderSteamId = "{SteamId}";
        private Game _game;
        private ScreenshotGroup _screenshotGroup;

        public ScreenshotUtilitiesSelfmadeProvider(IPlayniteAPI api) : base(api)
        {
            Settings = new ScreenshotUtilitiesSelfmadeProviderSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override Guid Id { get; } = Guid.Parse("a049eff8-fd41-4dbc-9e35-01acc6b1a0cb");
        public string ProviderName { get; set; } = "Self-made";
        public bool SupportsAutomaticScreenshots { get; set; } = true;
        public bool SupportsScreenshotSearch { get; set; } = false;
        private ScreenshotUtilitiesSelfmadeProviderSettingsViewModel Settings { get; set; }

        public async Task<bool> CleanUpAsync(Game game) => await ScreenshotHelper.DeleteOrphanedJsonFiles(game.Id, Id);

        public async Task<bool> GetScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate) => await FetchScreenshotsAsync(game, daysSinceLastUpdate, forceUpdate);

        public string GetScreenshotSearchResult(Game game, string searchTerm) => throw new NotImplementedException();

        public async Task<bool> GetScreenshotsManualAsync(Game game, string gameIdentifier) => throw new NotImplementedException();

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new SettingsView();

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                var notificationMessage = new NotificationMessage("Screenshot Utilities Selfmade Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);
            }
        }

        private async Task<bool> FetchScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate)
        {
            try
            {
                // TODO: Add method to refresh screenshots after a game was closed.

                // return when the main addon isn't installed.
                if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled || game == null)
                {
                    return false;
                }

                // set game and load the file.
                _game = game;

                var fileExists = false;

                (fileExists, _screenshotGroup) = ScreenshotHelper.LoadGroup(_game, ProviderName, Id);

                // return if we don't want to force an update and the last update was inside the
                // days configured.
                if (!forceUpdate
                    && _screenshotGroup.LastUpdate != null
                    && (_screenshotGroup.LastUpdate > DateTime.Now.AddDays(daysSinceLastUpdate * -1)))
                {
                    return false;
                }

                if (!fileExists)
                {
                    _screenshotGroup.GameIdentifier = _game.Id.ToString();
                }

                var updated = false;

                if (_game.PluginId == SteamHelper.SteamId)
                {
                    updated = LoadScreenshotsFromSteam();
                }

                ScreenshotHelper.SaveScreenshotGroupJson(game, _screenshotGroup);

                return updated;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error fetching screenshots for {game.Name}");
                return false;
            }
        }

        private bool LoadScreenshotsFromSteam()
        {
            var steamFolder = Settings.Settings.SteamPath.Replace(_placeholderSteamId, SteamHelper.GetSteamId(_game));

            var updated = false;

            try
            {
                var dirInfo = new DirectoryInfo(steamFolder);

                if (!dirInfo.Exists)
                {
                    return false;
                }

                var files = dirInfo.GetFiles("*.jpg").OrderBy(f => f.Name).ToList();

                if (_screenshotGroup.Screenshots.Count > 0)
                {
                    if (files.Count == 0)
                    {
                        _screenshotGroup.Screenshots.Clear();

                        return true;
                    }

                    _screenshotGroup.Screenshots.RemoveRange(_screenshotGroup.Screenshots.Where(s => !files.Any(f => f.FullName == s.Path)));
                }

                foreach (var file in files)
                {
                    var screenshot = _screenshotGroup.Screenshots.FirstOrDefault(s => s.Path == file.FullName);

                    if (screenshot != null)
                    {
                        screenshot.SortOrder = files.IndexOf(file);
                    }
                    else
                    {
                        _screenshotGroup.Screenshots.Add(new Screenshot(file.FullName)
                        {
                            ThumbnailPath = $"{steamFolder}\\thumbnails\\{file.Name}",
                            SortOrder = files.IndexOf(file),
                            Type = MediaType.SelfmadeScreenshot,
                            Name = file.Name
                        });
                    }

                    updated = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading screenshots from Steam for {_game.Name}");
            }

            return updated;
        }
    }
}
