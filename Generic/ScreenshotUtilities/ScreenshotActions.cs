using KNARZhelper;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using ScreenshotUtilities.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ScreenshotUtilities
{
    internal static class ScreenshotActions
    {
        internal static async Task<bool> DownloadScreenshotsAsync(Game game, ScreenshotUtilities plugin)
        {
            var groups = new ScreenshotGroups(plugin.GetPluginUserDataPath(), game.Id);

            return await groups.DownloadAllAsync(plugin.Settings.Settings.ThumbnailHeight);
        }

        internal static async Task<bool> GetScreenshotsAsync(Game game)
        {
            var needsRefresh = false;

            foreach (var plugin in API.Instance.Addons.Plugins)
            {
                var type = plugin.GetType();

                if (type != null && type.GetInterface("IScreenshotProvider") != null)
                {
                    var methodInfo = type.GetMethod("GetScreenshotsAsync");

                    if (methodInfo != null)
                    {
                        var parametersArray = new object[] { game };

                        var resultTask = (Task<bool>)methodInfo.Invoke(plugin, parametersArray);

                        needsRefresh |= await resultTask;
                    }
                }
            }

            return needsRefresh;
        }

        internal static void OpenScreenshotViewer(Game game, ScreenshotUtilities plugin)
        {
            var window = ScreenshotViewerViewModel.GetWindow(plugin, game);

            if (window == null)
            {
                return;
            }

            window.ShowDialog();
        }

        internal static async Task PrepareScreenshotsAsync(Game game, ScreenshotUtilities plugin)
        {
            try
            {
                if (plugin.Settings.Settings.Debug)
                {
                    API.Instance.MainView.UIDispatcher.Invoke(() => Log.Debug($"PrepareScreenshots {game.Name}: Getting screenshots"));
                }

                await GetScreenshotsAsync(game);

                var groups = new ScreenshotGroups();
                groups.CreateGroupsFromFiles(plugin.GetPluginUserDataPath(), game.Id, false);

                if (plugin.Settings.Settings.Debug)
                {
                    API.Instance.MainView.UIDispatcher.Invoke(() => Log.Debug($"PrepareScreenshots {game.Name}: Create groups: {groups?.Count}"));
                }

                if (groups.Count == 0)
                {
                    return;
                }

                if (plugin.Settings.Settings.AutomaticDownload)
                {

                    if (plugin.Settings.Settings.Debug)
                    {
                        API.Instance.MainView.UIDispatcher.Invoke(() => Log.Debug($"PrepareScreenshots {game.Name}: Downloading"));
                    }

                    if (((plugin.Settings.Settings.DownloadFilter.Count == 0)
                        || plugin.Settings.Settings.DownloadFilter.Any(f => f.ExistsInGame(game)))
                        && !groups.IsEverythingDownloaded)
                    {
                        await groups.DownloadAllAsync(plugin.Settings.Settings.ThumbnailHeight);
                    }
                }

                API.Instance.MainView.UIDispatcher.Invoke(() =>
                {
                    if (plugin.Settings.Settings.Debug)
                    {
                        Log.Debug($"PrepareScreenshots {game.Name}: Setting current groups: {groups?.Count}");
                    }

                    plugin.CurrentScreenshotsGroups = groups;

                    if (plugin.Settings.Settings.DisplayViewerControl)
                    {
                        plugin.Settings.Settings.IsViewerControlVisible = true;
                    }

                    plugin.RefreshControls();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error preparing screenshots for {game.Name}");
            }
        }

        internal static async Task<bool> RefreshThumbnailsAsync(Game game, ScreenshotUtilities plugin)
        {
            var groups = new ScreenshotGroups(plugin.GetPluginUserDataPath(), game.Id);

            return await groups.RefreshAllThumbnailsAsync(plugin.Settings.Settings.ThumbnailHeight);
        }
    }
}
