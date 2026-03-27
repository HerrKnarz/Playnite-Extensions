using KNARZhelper;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using ScreenshotUtilitiesLocalProvider.ViewModels;
using ScreenshotUtilitiesLocalProvider.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ScreenshotUtilitiesLocalProvider
{
    public class ScreenshotUtilitiesLocalProvider : GenericPlugin, IScreenshotProviderPlugin
    {
        public StringExpander StringExpander = new StringExpander();
        private Game _game;
        private ScreenshotGroup _screenshotGroup;

        public ScreenshotUtilitiesLocalProvider(IPlayniteAPI api) : base(api)
        {
            Settings = new SettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            Settings.Settings.FolderConfigs.ForEach(c => c.StringExpander = StringExpander);
        }

        public override Guid Id { get; } = Guid.Parse("a049eff8-fd41-4dbc-9e35-01acc6b1a0cb");

        public string ProviderName { get; set; } = "Local";

        public bool SupportsAutomaticScreenshots { get; set; } = true;

        public bool SupportsScreenshotSearch { get; set; } = false;

        private SettingsViewModel Settings { get; set; }

        public async Task<bool> CleanUpAsync(Game game) => await ScreenshotHelper.DeleteOrphanedJsonFiles(game.Id, Id);

        public async Task<bool> GetScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate) => await FetchScreenshotsAsync(game, daysSinceLastUpdate, forceUpdate);

        public string GetScreenshotSearchResult(Game game, string searchTerm) => throw new NotImplementedException();

        public async Task<bool> GetScreenshotsManualAsync(Game game, string gameIdentifier) => throw new NotImplementedException();

        public override ISettings GetSettings(bool firstRunSettings) => Settings;

        public override UserControl GetSettingsView(bool firstRunSettings) => new SettingsView();

        public async Task<bool> HandleGameStoppedAsync(Game game) => await FetchScreenshotsAsync(game, 0, true);

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                var notificationMessage = new NotificationMessage("Screenshot Utilities Local Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);

                return;
            }

            Settings.Settings.GameProfiles.ForEach(p => p.PrepareProfile(StringExpander, p.GameId));
        }

        private async Task<bool> FetchScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate)
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

                var screenshots = new List<Screenshot>();

                foreach (var folderConfig in Settings.Settings.FolderConfigs.Where(c => c.Active))
                {
                    screenshots.AddRange(folderConfig.LoadScreenshots(_game));
                }

                var screenshotsToRemove = _screenshotGroup.Screenshots.Where(s => !screenshots.Any(f => f.Path == s.Path)).ToList();
                var screenshotsToAdd = screenshots.Where(s => !_screenshotGroup.Screenshots.Any(f => f.Path == s.Path)).ToList();

                _screenshotGroup.Screenshots.RemoveRange(screenshotsToRemove);

                _screenshotGroup.Screenshots.AddRange(screenshotsToAdd);

                var sortOrder = 0;

                foreach (var screenshot in _screenshotGroup.Screenshots.OrderBy(s => s.Name))
                {
                    sortOrder++;
                    screenshot.SortOrder = sortOrder;
                }

                ScreenshotHelper.SaveScreenshotGroupJson(game, _screenshotGroup);

                return screenshotsToRemove.Count > 0 || screenshotsToAdd.Count > 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error fetching screenshots for {game.Name}");
                return false;
            }
        }
    }
}
