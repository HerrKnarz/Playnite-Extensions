using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ScreenshotUtilities.Models
{
    public class ScreenshotProviderPlugin : IScreenshotProviderPlugin
    {
        private readonly MethodInfo _methodInfoCleanUpAsync = null;
        private readonly MethodInfo _methodInfoGetScreenshotsAsync = null;
        private readonly MethodInfo _methodInfoGetScreenshotSearchResults = null;
        private readonly MethodInfo _methodInfoGetScreenshotsManualAsync = null;
        private readonly Plugin _plugin;
        private Game _game = null;
        private ScreenshotSearchResult _screenshotSearchResult = null;

        public ScreenshotProviderPlugin(Plugin plugin)
        {
            _plugin = plugin;

            var type = _plugin.GetType();

            var propertyInfoName = type.GetProperty("Name");

            Name = propertyInfoName != null ? (string)propertyInfoName.GetValue(_plugin, null) : type.Name;

            _methodInfoCleanUpAsync = type.GetMethod("CleanUpAsync");
            _methodInfoGetScreenshotsAsync = type.GetMethod("GetScreenshotsAsync");
            _methodInfoGetScreenshotSearchResults = type.GetMethod("GetScreenshotSearchResult");
            _methodInfoGetScreenshotsManualAsync = type.GetMethod("GetScreenshotsManualAsync");

            var propertyInfoAutomaticScreenshots = type.GetProperty("SupportsAutomaticScreenshots");

            if (propertyInfoAutomaticScreenshots != null)
            {
                SupportsAutomaticScreenshots = (bool)propertyInfoAutomaticScreenshots.GetValue(_plugin, null);
            }

            var propertyInfoScreenshotSearch = type.GetProperty("SupportsScreenshotSearch");

            if (propertyInfoScreenshotSearch != null)
            {
                SupportsScreenshotSearch = (bool)propertyInfoScreenshotSearch.GetValue(_plugin, null);
            }
        }

        public Guid Id => _plugin.Id;
        public string Name { get; set; } = null;
        public bool SupportsAutomaticScreenshots { get; set; } = false;
        public bool SupportsScreenshotSearch { get; set; } = false;

        public async Task<bool> CleanUpAsync(Game game)
        {
            var parametersArray = new object[] { game };

            var resultTask = (Task<bool>)_methodInfoCleanUpAsync.Invoke(_plugin, parametersArray);

            return await resultTask;
        }

        public async Task<bool> GetScreenshotsAsync(Game game, int daysSinceLastUpdate, bool forceUpdate)
        {
            if (!SupportsAutomaticScreenshots || _methodInfoGetScreenshotsAsync == null)
            {
                return false;
            }

            var parametersArray = new object[] { game, daysSinceLastUpdate, forceUpdate };

            var resultTask = (Task<bool>)_methodInfoGetScreenshotsAsync.Invoke(_plugin, parametersArray);

            return await resultTask;
        }

        public string GetScreenshotSearchResult(Game game, string searchTerm)
        {
            if (!SupportsScreenshotSearch || _methodInfoGetScreenshotSearchResults == null)
            {
                return null;
            }

            var parametersArray = new object[] { game, searchTerm };

            return (string)_methodInfoGetScreenshotSearchResults.Invoke(_plugin, parametersArray);
        }

        public async Task<bool> GetScreenshotsManualAsync(Game game, string gameIdentifier)
        {
            if (!SupportsScreenshotSearch || _methodInfoGetScreenshotsManualAsync == null)
            {
                return false;
            }

            var parametersArray = new object[] { game, gameIdentifier == default ? _screenshotSearchResult.Identifier : gameIdentifier };

            var resultTask = (Task<bool>)_methodInfoGetScreenshotsManualAsync.Invoke(_plugin, parametersArray);

            return await resultTask;
        }

        public List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            var genericResult = new List<GenericItemOption>();

            var resultString = GetScreenshotSearchResult(_game, searchTerm);

            if (string.IsNullOrEmpty(resultString))
            {
                return genericResult;
            }

            var searchResult = Serialization.FromJson<List<ScreenshotSearchResult>>(resultString);

            if (searchResult == null)
            {
                return genericResult;
            }

            genericResult.AddRange(searchResult);

            return genericResult;
        }

        public void Search(Game game)
        {
            _game = game;

            _screenshotSearchResult = (ScreenshotSearchResult)API.Instance.Dialogs.ChooseItemWithSearch(
                new List<GenericItemOption>(),
                GetSearchResults,
                _game.Name,
                $"{ResourceProvider.GetString("LOCLinkUtilitiesDialogSearchGame")} ({_plugin})");
        }
    }
}