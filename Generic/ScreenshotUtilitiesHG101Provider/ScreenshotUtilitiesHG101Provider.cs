using KNARZhelper;
using KNARZhelper.MetadataCommon;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Threading.Tasks;

namespace ScreenshotUtilitiesHG101Provider
{
    public class ScreenshotUtilitiesHG101Provider : GenericPlugin, IScreenshotProviderPlugin
    {
        public HG101Parser HG101Parser = new HG101Parser();
        private const string _websiteName = "Hardcore Gaming 101";
        private const string _websiteUrl = "http://www.hardcoregaming101.net/";

        public ScreenshotUtilitiesHG101Provider(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public override Guid Id { get; } = Guid.Parse("dde4dbbb-d889-4b48-aeda-e663a6e11c1a");
        public string ProviderName { get; set; } = _websiteName;
        public bool SupportsAutomaticScreenshots { get; set; } = true;
        public bool SupportsScreenshotSearch { get; set; } = true;

        public async Task<bool> CleanUpAsync(Game game) => await ScreenshotHelper.DeleteOrphanedJsonFiles(game.Id, Id);

        public async Task<bool> FetchScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate, string identifier = default)
        {
            try
            {
                // return when the main addon isn't installed.
                if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled || game == null)
                {
                    return false;
                }

                var fileExists = false;
                ScreenshotGroup screenshotGroup;

                (fileExists, screenshotGroup) = ScreenshotHelper.LoadGroup(game, _websiteName, Id);

                // return if we don't want to force an update and the last update was inside the
                // days configured.
                if (!forceUpdate
                    && screenshotGroup.LastUpdate != null
                    && (screenshotGroup.LastUpdate > DateTime.Now.AddDays(daysSinceLastUpdate * -1)))
                {
                    return false;
                }

                // Return if a game was searched and it's the one we already have.
                if (identifier != default && identifier.Equals(screenshotGroup.GameIdentifier))
                {
                    return false;
                }

                // Get the right name to search for.
                var searchName = GetIdentifier(game, screenshotGroup, identifier);

                if (string.IsNullOrEmpty(searchName))
                {
                    return false;
                }

                // We need to reset the file if we got a new gogId from the method call and it's not
                // the same we already got.
                if (!fileExists || (identifier != default && !searchName.Equals(screenshotGroup.GameIdentifier)))
                {
                    screenshotGroup.GameIdentifier = searchName;

                    screenshotGroup.Screenshots.Clear();
                }

                var updated = await HG101Parser.LoadScreenshotsAsync(screenshotGroup);

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

        public string GetScreenshotSearchResult(Game game, string searchTerm) => HG101Parser.GetScreenshotSearchResult(searchTerm);

        public async Task<bool> GetScreenshotsManualAsync(Game game, string gameIdentifier) => await FetchScreenshotsAsync(game, 0, true, gameIdentifier);

        public async Task<bool> HandleGameStoppedAsync(Game game) => false;

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!ScreenshotHelper.IsScreenshotUtilitiesInstalled)
            {
                var notificationMessage = new NotificationMessage("Screenshot Utilities Hardcore Gaming 101 Provider", "Screenshot Utilities has to be installed for this addon to work!", NotificationType.Error);

                PlayniteApi.Notifications.Add(notificationMessage);
            }
        }

        private string GetIdentifier(Game game, ScreenshotGroup screenshotGroup, string identifier = default)
        {
            var searchIdentifier = identifier;

            if (searchIdentifier == default)
            {
                searchIdentifier = screenshotGroup.GameIdentifier;
            }

            if (searchIdentifier == default)
            {
                // Get the right name to search for.
                searchIdentifier = MetadataHelper.GetLink(game, new System.Text.RegularExpressions.Regex(@"hardcoregaming101\.net"))?.Url ?? default;
            }

            if (searchIdentifier == default)
            {
                // Get the right name to search for.
                var searchName = game.Name
                        .RemoveSpecialChars()
                        .CollapseWhitespaces()
                        .Replace(" ", "-")
                        .ToLower();

                if (!string.IsNullOrEmpty(searchName))
                {
                    searchIdentifier = $"{_websiteUrl}{searchName}";
                }
            }

            return searchIdentifier;
        }
    }
}
