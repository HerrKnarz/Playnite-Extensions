using KNARZhelper;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using ScreenshotUtilities.Models;
using ScreenshotUtilities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ScreenshotUtilities
{
    public enum ActionModifierType
    {
        None,
        Download,
        RefreshScreenshots,
        RefreshThumbnails,
        Reset,
        Ignore
    }

    internal static class ScreenshotActions
    {
        internal static void DoForAll(List<Game> games, ScreenshotUtilities plugin,
            ActionModifierType actionModifier = ActionModifierType.None, Guid providerId = default, bool completeLibrary = false)
        {
            if (games == null || games.Count == 0)
            {
                return;
            }

            if (games.Count == 1)
            {
                switch (actionModifier)
                {
                    case ActionModifierType.Download:
                        plugin.DownloadScreenshotsAsync(games[0], providerId);
                        break;

                    case ActionModifierType.RefreshScreenshots:
                        plugin.GetScreenshotsAsync(games[0], providerId);
                        break;

                    case ActionModifierType.RefreshThumbnails:
                        plugin.RefreshThumbnailsAsync(games[0], providerId);
                        break;

                    case ActionModifierType.Reset:
                        plugin.ResetScreenshotsAsync(games[0], providerId);
                        break;

                    case ActionModifierType.Ignore:
                        SetGameToIgnore(games[0], plugin, providerId);
                        break;
                }

                return;
            }

            var gamesAffected = 0;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var progressMessage = string.Empty;
                var resultMessage = string.Empty;

                switch (actionModifier)
                {
                    case ActionModifierType.Download:
                        progressMessage = ResourceProvider.GetString("LOCScreenshotUtilitiesProgressDownload");
                        resultMessage = "LOCScreenshotUtilitiesResultDownload";
                        break;

                    case ActionModifierType.RefreshScreenshots:
                        progressMessage = ResourceProvider.GetString("LOCScreenshotUtilitiesProgressRefreshScreenshots");
                        resultMessage = "LOCScreenshotUtilitiesResultRefreshScreenshots";
                        break;

                    case ActionModifierType.RefreshThumbnails:
                        progressMessage = ResourceProvider.GetString("LOCScreenshotUtilitiesProgressRefreshThumbnails");
                        resultMessage = "LOCScreenshotUtilitiesResultRefreshThumbnails";
                        break;

                    case ActionModifierType.Reset:
                        progressMessage = ResourceProvider.GetString("LOCScreenshotUtilitiesProgressReset");
                        resultMessage = "LOCScreenshotUtilitiesResultReset";

                        var warningMessage = completeLibrary
                            ? ResourceProvider.GetString("LOCScreenshotUtilitiesDialogResetScreenshotsAll")
                            : ResourceProvider.GetString("LOCScreenshotUtilitiesDialogResetScreenshots");

                        if (API.Instance.Dialogs.ShowMessage(warningMessage, string.Empty, MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.No)
                        {
                            return;
                        }

                        break;

                    case ActionModifierType.Ignore:
                        progressMessage = ResourceProvider.GetString("LOCScreenshotUtilitiesProgressDisable");
                        resultMessage = "LOCScreenshotUtilitiesResultDisable";
                        break;
                }

                using (API.Instance.Database.BufferedUpdate())
                {
                    var globalProgressOptions = new GlobalProgressOptions(
                        $"{ResourceProvider.GetString("LOCScreenshotUtilitiesName")} - {progressMessage}",
                        true
                    )
                    {
                        IsIndeterminate = false
                    };

                    API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                    {
                        try
                        {
                            activateGlobalProgress.ProgressMaxValue = games.Count;

                            foreach (var game in games)
                            {
                                activateGlobalProgress.Text =
                                    $"{ResourceProvider.GetString("LOCScreenshotUtilitiesName")}{Environment.NewLine}{progressMessage}{Environment.NewLine}{game.Name}";

                                if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                switch (actionModifier)
                                {
                                    case ActionModifierType.Download:
                                        if (AsyncHelper.RunSync(async () => await DownloadScreenshotsAsync(game, plugin, providerId)))
                                        {
                                            gamesAffected++;
                                        }

                                        break;

                                    case ActionModifierType.RefreshScreenshots:
                                        if (AsyncHelper.RunSync(async () => await GetScreenshotsAsync(game, plugin, true, providerId)))
                                        {
                                            gamesAffected++;
                                        }

                                        break;

                                    case ActionModifierType.RefreshThumbnails:
                                        if (AsyncHelper.RunSync(async () =>
                                            await RefreshThumbnailsAsync(game, plugin, providerId)))
                                        {
                                            gamesAffected++;
                                        }

                                        break;

                                    case ActionModifierType.Reset:
                                        var isSelected = game.Id == API.Instance.MainView.SelectedGames.FirstOrDefault().Id;

                                        if (AsyncHelper.RunSync(async () => await ResetScreenshotsAsync(game, plugin, providerId, isSelected)))
                                        {
                                            gamesAffected++;
                                        }

                                        break;

                                    case ActionModifierType.Ignore:
                                        SetGameToIgnore(game, plugin, providerId, false);
                                        gamesAffected++;
                                        break;
                                }

                                activateGlobalProgress.CurrentProgressValue++;
                            }

                            if (gamesAffected > 0)
                            {
                                plugin.RefreshControls();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }, globalProgressOptions);
                }

                Cursor.Current = Cursors.Default;
                API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString(resultMessage), gamesAffected));
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        internal static async Task<bool> DownloadScreenshotsAsync(Game game, ScreenshotUtilities plugin, Guid providerGuid = default)
        {
            var groups = new ScreenshotGroups(plugin.GetPluginUserDataPath(), game.Id);

            return await groups.DownloadAllAsync(plugin.Settings.Settings.ThumbnailHeight, new HashSet<Guid> { providerGuid });
        }

        internal static async Task<bool> GetScreenshotsAsync(Game game, ScreenshotUtilities plugin, bool forceUpdate = false, Guid providerId = default)
        {
            var needsRefresh = false;

            if (!plugin.ProvidersInitialized)
            {
                InitializeProviders(plugin);
            }

            var screenshotGroups = new ScreenshotGroups(plugin.GetPluginUserDataPath(), game.Id);

            var firstProviderWithScreenshots = Guid.Empty;

            foreach (var provider in plugin.ScreenshotProviders
                .Where(p => p.SupportsAutomaticScreenshots && (providerId == default || p.Id == providerId))
                .OrderBy(p => plugin.Settings.Settings.ProviderSettings[p.ProviderName].Priority)
                .ThenBy(p => p.ProviderName))
            {
                // Skip providers that are set to only fetch manually if this is not a forced update
                if (plugin.Settings.Settings.ProviderSettings[provider.ProviderName].FetchMode == ScreenshotFetchMode.OnlyManually && !forceUpdate)
                {
                    continue;
                }

                var existingGroup = screenshotGroups.FirstOrDefault(g => g.Provider?.Id == provider.Id);

                if (existingGroup?.Provider?.Id == provider.Id)
                {
                    // Remember the first provider with screenshots to allow skipping providers that
                    // are set to only fetch if first if they don't have any screenshots and aren't
                    // the first provider with screenshots
                    if (existingGroup.Screenshots.Count > 0 && firstProviderWithScreenshots == Guid.Empty)
                    {
                        firstProviderWithScreenshots = provider.Id;
                    }

                    // Skip providers that are set to ignore the game or the last update is within
                    // the refresh interval if it's not a forced update
                    if (existingGroup.IgnoreGame || (!forceUpdate
                        && existingGroup.LastUpdate != null
                        && (existingGroup.LastUpdate > DateTime.Now.AddDays(plugin.Settings.Settings.ProviderSettings[provider.ProviderName].DaysUntilRefresh * -1))))
                    {
                        continue;
                    }
                }

                // Skip providers that are set to only fetch if first if they don't have any
                // screenshots and aren't the first provider with screenshots
                if ((existingGroup?.Screenshots.Count ?? 0) == 0
                    && firstProviderWithScreenshots != provider.Id
                    && firstProviderWithScreenshots != Guid.Empty
                    && plugin.Settings.Settings.ProviderSettings[provider.ProviderName].FetchMode == ScreenshotFetchMode.OnlyIfFirst)
                {
                    continue;
                }

                needsRefresh |= await provider.CleanUpAsync(game);
                needsRefresh |= await provider.GetScreenshotsAsync(game, plugin.Settings.Settings.ProviderSettings[provider.ProviderName].DaysUntilRefresh, forceUpdate);
            }

            return needsRefresh;
        }

        internal static async Task HandleGameStoppedAsync(ScreenshotUtilities plugin, Game game)
        {
            var needsRefresh = false;

            foreach (var provider in plugin.ScreenshotProviders)
            {
                needsRefresh |= await provider.HandleGameStoppedAsync(game);
            }

            if (needsRefresh)
            {
                PrepareScreenshotsAsync(game, plugin, false);
            }
        }

        internal static void InitializeProviders(ScreenshotUtilities plugin)
        {
            if (plugin.ProvidersInitialized)
            {
                return;
            }

            plugin.ScreenshotProviders.Clear();

            plugin.Settings.Settings.ProviderSettings = plugin.Settings.Settings.ProviderSettings ?? new Dictionary<string, ProviderSettings>();

            foreach (var provider in API.Instance.Addons.Plugins)
            {
                var type = provider.GetType();

                if ((type == null) || (type.GetInterface("IScreenshotProviderPlugin") == null))
                {
                    continue;
                }

                var screenshotProvider = new ScreenshotProviderPlugin(provider);

                plugin.ScreenshotProviders.Add(screenshotProvider);

                if (!plugin.Settings.Settings.ProviderSettings.ContainsKey(screenshotProvider.ProviderName))
                {
                    plugin.Settings.Settings.ProviderSettings[screenshotProvider.ProviderName] = new ProviderSettings();
                }
            }

            plugin.Settings.Settings.ProviderSettings.RemoveAll(x => plugin.ScreenshotProviders.All(p => p.ProviderName != x.Key));

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

                var providersToDownload = plugin.ScreenshotProviders
                    .Where(p => plugin.Settings.Settings.ProviderSettings[p.ProviderName].DownloadAutomatically
                        && plugin.ScreenshotProviders.Any(sp => sp.Id == p.Id && !sp.IsLocalProvider))
                    .Select(p => p.Id)
                    .ToHashSet();

                if (providersToDownload.Count > 0)
                {
                    if (plugin.Settings.Settings.Debug)
                    {
                        API.Instance.MainView.UIDispatcher.Invoke(() => Log.Debug($"PrepareScreenshots {game.Name}: Downloading"));
                    }

                    if (((plugin.Settings.Settings.DownloadFilter.Count == 0)
                        || plugin.Settings.Settings.DownloadFilter.Any(f => f.ExistsInGame(game)))
                        && !groups.IsEverythingDownloaded)
                    {
                        await groups.DownloadAllAsync(plugin.Settings.Settings.ThumbnailHeight, providersToDownload);
                    }
                }

                var providersToCreateThumbnails = plugin.ScreenshotProviders
                    .Where(p => plugin.Settings.Settings.ProviderSettings[p.ProviderName].AlwaysCreateThumbnails && !plugin.Settings.Settings.ProviderSettings[p.ProviderName].DownloadAutomatically)
                    .Select(p => p.Id)
                    .ToHashSet();

                if (providersToCreateThumbnails.Count > 0)
                {
                    if (plugin.Settings.Settings.Debug)
                    {
                        API.Instance.MainView.UIDispatcher.Invoke(() => Log.Debug($"PrepareScreenshots {game.Name}: Creating thumbnails"));
                    }

                    foreach (var group in groups)
                    {
                        if (providersToCreateThumbnails.Contains(group.Provider.Id))
                        {
                            //TODO: Temporary Logging! Remove later!
                            Log.Debug($"PrepareScreenshots {game.Name}: THUMBDEBUG: found {group.Provider.Name}");
                            await group.RefreshThumbnailsAsync(plugin.Settings.Settings.ThumbnailHeight, false, true);
                        }
                    }
                }

                groups.DeleteOrphanedFiles();
                groups.ForEach(g => g.SortOrder = plugin.Settings.Settings.ProviderSettings[g.Provider.Name].SortOrder);
                groups.Sort(g => g.Name);
                groups.Sort(g => g.SortOrder);

                var updateGame = false;
                updateGame = SetTags(game, plugin, groups);

                if (plugin.Settings.Settings.Debug)
                {
                    Log.Debug($"PrepareScreenshots {game.Name}: Setting current groups: {groups?.Count}");
                }

                API.Instance.MainView.UIDispatcher.Invoke(() =>
                {
                    try
                    {
                        if (updateGame)
                        {
                            API.Instance.Database.Games.Update(game);
                        }

                        plugin.Settings.Settings.CurrentScreenshotGroups = groups;

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

            return await groups.RefreshAllThumbnailsAsync(plugin.Settings.Settings.ThumbnailHeight,
                providerId,
                plugin.Settings.Settings.ProviderSettings[plugin.ScreenshotProviders.First(p => p.Id == providerId).ProviderName].AlwaysCreateThumbnails);
        }

        internal static async Task<bool> ResetScreenshotsAsync(Game game, ScreenshotUtilities plugin, Guid providerId = default, bool refreshScreenshots = true)
        {
            if (!ScreenshotHelper.RemoveScreenshots(game, true, providerId))
            {
                return false;
            }

            if (refreshScreenshots)
            {
                await PrepareScreenshotsAsync(game, plugin, true);
            }

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

        internal static void SetGameToIgnore(Game game, ScreenshotUtilities plugin, Guid providerId = default, bool refreshCotrols = true)
        {
            var groups = new ScreenshotGroups(plugin.GetPluginUserDataPath(), game.Id);

            if (groups == null || groups.Count == 0)
            {
                return;
            }

            foreach (var group in groups.Where(g => providerId == default || g.Provider.Id == providerId))
            {
                group.IgnoreGame = true;
                group.Screenshots.Clear();
                group.Save();
            }

            if (!refreshCotrols)
            {
                plugin.RefreshControls();
            }
        }

        private static bool SetTags(Game game, ScreenshotUtilities plugin, ScreenshotGroups groups)
        {
            var updateGame = false;

            foreach (var provider in plugin.ScreenshotProviders)
            {
                var tagToSet = plugin.Settings.Settings.ProviderSettings[provider.ProviderName].TagWhenHavingScreenshots;

                if (string.IsNullOrEmpty(tagToSet))
                {
                    continue;
                }

                var tagId = new TypeTag().AddDbObject(tagToSet);

                var group = groups.FirstOrDefault(g => g.Provider.Id == provider.Id);

                if (group == null || group.Screenshots.Count == 0)
                {
                    updateGame |= game.TagIds.Remove(tagId);
                }
                else
                {
                    if (!game.TagIds.Contains(tagId))
                    {
                        game.TagIds.Add(tagId);

                        updateGame = true;
                    }
                }
            }

            return updateGame;
        }
    }
}
