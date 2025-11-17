using KNARZhelper;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using ScreenshotUtilities.Models;
using ScreenshotUtilities.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ScreenshotUtilities
{
    internal static class ScreenshotActions
    {
        internal static int DaysSinceLastUpdate = 5;

        internal static async Task<bool> DownloadScreenshotsAsync(Game game, ScreenshotUtilities plugin, Guid providerGuid = default)
        {
            var groups = new ScreenshotGroups(plugin.GetPluginUserDataPath(), game.Id);

            return await groups.DownloadAllAsync(plugin.Settings.Settings.ThumbnailHeight, providerGuid);
        }

        internal static async Task<bool> GetScreenshotsAsync(Game game, ScreenshotUtilities plugin, bool forceUpdate = false, Guid providerId = default)
        {
            var needsRefresh = false;

            if (!plugin.ProvidersInitialized)
            {
                InitializeProviders(plugin);
            }

            var screenshotGroups = new ScreenshotGroups(plugin.GetPluginUserDataPath(), game.Id);

            foreach (var provider in plugin.ScreenshotProviders.Where(p => p.SupportsAutomaticScreenshots && (providerId == default || p.Id == providerId)))
            {
                var existingGroup = screenshotGroups.FirstOrDefault(g => g.Provider.Id == provider.Id);

                if (existingGroup?.Provider.Id == provider.Id)
                {
                    if (existingGroup.IgnoreGame || (!forceUpdate
                        && existingGroup.LastUpdate != null
                        && (existingGroup.LastUpdate > DateTime.Now.AddDays(DaysSinceLastUpdate * -1))))
                    {
                        continue;
                    }
                }

                needsRefresh |= await provider.CleanUpAsync(game);
                needsRefresh |= await provider.GetScreenshotsAsync(game, DaysSinceLastUpdate, forceUpdate);
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

        internal static async Task PrepareScreenshotsAsync(Game game, ScreenshotUtilities plugin, bool getFromSource = true)
        {
            try
            {
                if (plugin.Settings.Settings.Debug)
                {
                    API.Instance.MainView.UIDispatcher.Invoke(() => Log.Debug($"PrepareScreenshots {game.Name}: Getting screenshots"));
                }

                if (getFromSource)
                {
                    await GetScreenshotsAsync(game, plugin);
                }

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

                groups.DeleteOrphanedFiles();

                API.Instance.MainView.UIDispatcher.Invoke(() =>
                {
                    try
                    {
                        if (plugin.Settings.Settings.Debug)
                        {
                            Log.Debug($"PrepareScreenshots {game.Name}: Setting current groups: {groups?.Count}");
                        }

                        plugin.CurrentScreenshotsGroups = groups;

                        if (plugin.Settings.Settings.DisplayViewerControl && groups.ScreenshotCount > 0)
                        {
                            plugin.Settings.Settings.IsViewerControlVisible = true;
                        }

                        plugin.RefreshControls();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error updating screenshot viewer controls for {game.Name}");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error preparing screenshots for {game.Name}");
            }
        }

        internal static async Task<bool> RefreshThumbnailsAsync(Game game, ScreenshotUtilities plugin, Guid providerId = default)
        {
            var groups = new ScreenshotGroups(plugin.GetPluginUserDataPath(), game.Id);

            return await groups.RefreshAllThumbnailsAsync(plugin.Settings.Settings.ThumbnailHeight, providerId);
        }

        internal static async Task<bool> ResetScreenshots(Game game, ScreenshotUtilities plugin, Guid providerId = default)
        {
            if (!ScreenshotHelper.RemoveScreenshots(game, true, providerId))
            {
                return false;
            }

            await PrepareScreenshotsAsync(game, plugin, true);

            return true;
        }

        internal static async Task<bool> SearchScreenshotsAsync(Game game, ScreenshotUtilities plugin, Guid providerId = default)
        {
            var needsRefresh = false;

            if (!plugin.ProvidersInitialized)
            {
                InitializeProviders(plugin);
            }

            var providers = plugin.ScreenshotProviders.Where(p => p.SupportsScreenshotSearch && (providerId == default || p.Id == providerId)).ToList();

            foreach (var provider in providers)
            {
                provider.Search(game);
            }

            foreach (var provider in providers)
            {
                needsRefresh |= await provider.GetScreenshotsManualAsync(game, null);
            }

            if (needsRefresh)
            {
                PrepareScreenshotsAsync(game, plugin, false);
            }

            return needsRefresh;
        }

        internal static void SetGameToIgnore(Game game, ScreenshotUtilities plugin, Guid providerId = default)
        {
            if (plugin.CurrentScreenshotsGroups == null || plugin.CurrentScreenshotsGroups.Count == 0)
            {
                return;
            }

            foreach (var group in plugin.CurrentScreenshotsGroups.Where(g => providerId == default || g.Provider.Id == providerId))
            {
                group.IgnoreGame = true;
                group.Screenshots.Clear();
                group.Save();
            }

            plugin.RefreshControls();
        }
    }
}