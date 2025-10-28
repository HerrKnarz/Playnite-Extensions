using KNARZhelper;
using KNARZhelper.FilesCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using ScreenshotUtilities.Models;
using ScreenshotUtilities.ViewModels;
using System;
using System.Diagnostics;
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

        internal static async Task<bool> GetScreenshotsAsync(Game game, ScreenshotUtilities plugin, bool forceUpdate = false)
        {
            var needsRefresh = false;

            if (!plugin.ProvidersInitialized)
            {
                InitializeProviders(plugin);
            }

            foreach (var provider in plugin.ScreenshotProviders.Where(p => p.SupportsAutomaticScreenshots))
            {
                needsRefresh |= await provider.GetScreenshotsAsync(game, 5, forceUpdate);
            }

            return needsRefresh;
        }

        internal static void InitializeProviders(ScreenshotUtilities plugin)
        {
            if (plugin.ProvidersInitialized)
            {
                return;
            }

            plugin.ScreenshotProviders.Clear();

            foreach (var provider in API.Instance.Addons.Plugins)
            {
                var type = provider.GetType();

                if ((type == null) || (type.GetInterface("IScreenshotProviderPlugin") == null))
                {
                    continue;
                }

                plugin.ScreenshotProviders.Add(new ScreenshotProviderPlugin(provider));
            }

            plugin.ProvidersInitialized = true;
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

                await GetScreenshotsAsync(game, plugin);

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

        internal static void RemoveScreenshots(Game game, ScreenshotUtilities plugin)
        {
            var path = FileHelper.GetDownloadPath(plugin.GetPluginUserDataPath(), game.Id);
            var succeeded = false;
            try
            {
                if (path.Exists)
                {
                    path.Delete(true);
                }

                Task.Delay(TimeSpan.FromMilliseconds(100));

                path.Refresh();

                succeeded = !path.Exists;
            }
            catch (Exception ex)
            {
                succeeded = false;
                Log.Error(ex, $"Couldn't delete folder for removed game \"{game.Name}\" ({game.Id})");
            }

            if (!succeeded)
            {
                API.Instance.Notifications.Add(new NotificationMessage($"ScreenshotUtilitiesDelete{path.Name}",
                    string.Format(ResourceProvider.GetString("LOCScreenshotUtilitiesNotificationFolderNotDeleted"), game.Name),
                    NotificationType.Error,
                    () => Process.Start("explorer.exe", path.FullName)));
            }
        }

        internal static async Task<bool> SearchScreenshotsAsync(Game game, ScreenshotUtilities plugin)
        {
            var needsRefresh = false;

            if (!plugin.ProvidersInitialized)
            {
                InitializeProviders(plugin);
            }

            foreach (var provider in plugin.ScreenshotProviders.Where(p => p.SupportsScreenshotSearch))
            {
                provider.Search(game);
            }

            foreach (var provider in plugin.ScreenshotProviders.Where(p => p.SupportsScreenshotSearch))
            {
                needsRefresh |= await provider.GetScreenshotsManualAsync(game, null);
            }

            return needsRefresh;
        }
    }
}
