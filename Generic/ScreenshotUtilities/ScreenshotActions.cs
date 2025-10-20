using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using ScreenshotUtilities.ViewModels;

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

        internal static void OpenScreenshotViewer(Game game, ScreenshotUtilities plugin)
        {
            var window = ScreenshotViewerViewModel.GetWindow(plugin, game);

            if (window == null)
            {
                return;
            }

            window.ShowDialog();
        }

        internal static bool RefreshThumbnails(Game game, ScreenshotUtilities plugin)
        {
            var groups = new ScreenshotGroups(plugin.GetPluginUserDataPath(), game.Id);

            return groups.RefreshAllThumbnails(plugin.Settings.Settings.ThumbnailHeight);
        }
    }
}
