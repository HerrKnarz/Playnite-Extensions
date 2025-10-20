using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using ScreenshotUtilities.ViewModels;
using System.Linq;

namespace ScreenshotUtilities
{
    internal static class ScreenshotActions
    {
        internal static bool DownloadScreenshots(Game game, ScreenshotUtilities plugin)
        {
            var groups = new ScreenshotGroups(plugin.GetPluginUserDataPath(), game.Id);

            return groups.DownloadAll(plugin.Settings.Settings.ThumbnailHeight);
        }

        internal static bool GetScreenshots(Game game)
        {
            var needsRefresh = false;

            foreach (var plugin in API.Instance.Addons.Plugins)
            {
                var type = plugin.GetType();

                if (type != null && type.GetInterface("IScreenshotProvider") != null)
                {
                    var methodInfo = type.GetMethod("GetScreenshots");

                    if (methodInfo != null)
                    {
                        var parametersArray = new object[] { game };

                        needsRefresh |= (bool)methodInfo.Invoke(plugin, parametersArray);
                    }
                }
            }

            return needsRefresh;
        }

        public static ScreenshotGroups LoadScreenshots(Game game, ScreenshotUtilities plugin, bool standaloneMode = false)
        {
            var groups = new ScreenshotGroups();
            groups.CreateGroupsFromFiles(plugin.GetPluginUserDataPath(), game.Id, false);

            return groups;
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

        internal static void PrepareScreenshots(Game game, ScreenshotUtilities plugin)
        {
            plugin.Settings.Settings.IsViewerControlVisible = false;
            plugin.CurrentScreenshotsGroups.Reset();

            GetScreenshots(game);

            var groups = new ScreenshotGroups();
            groups.CreateGroupsFromFiles(plugin.GetPluginUserDataPath(), game.Id, false);

            if (plugin.Settings.Settings.AutomaticDownload)
            {

                if (((plugin.Settings.Settings.DownloadFilter.Count == 0)
                    || plugin.Settings.Settings.DownloadFilter.Any(f => f.ExistsInGame(game)))
                    && !groups.IsEverythingDownloaded)
                {
                    groups.DownloadAll(plugin.Settings.Settings.ThumbnailHeight);
                }
            }

            if (groups.Count == 0)
            {
                return;
            }

            plugin.CurrentScreenshotsGroups = groups;

            if (plugin.Settings.Settings.DisplayViewerControl)
            {
                plugin.Settings.Settings.IsViewerControlVisible = true;
            }
        }

        internal static bool RefreshThumbnails(Game game, ScreenshotUtilities plugin)
        {
            var groups = new ScreenshotGroups(plugin.GetPluginUserDataPath(), game.Id);

            return groups.RefreshAllThumbnails(plugin.Settings.Settings.ThumbnailHeight);
        }
    }
}
