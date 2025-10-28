using KNARZhelper.ScreenshotsCommon;
using KNARZhelper.ScreenshotsCommon.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ScreenshotUtilities.Models
{
    public class ScreenshotProviderPlugin : IScreenshotProviderPlugin
    {
        private readonly Plugin _plugin;
        private readonly MethodInfo _methodInfoGetScreenshotsAsync = null;
        private readonly MethodInfo _methodInfoGetScreenshotSearchResults = null;
        private readonly MethodInfo _methodInfoGetScreenshotsManualAsync = null;

        private Game _game = null;
        private ScreenshotSearchResult _screenshotSearchResult = null;

        public ScreenshotProviderPlugin(Plugin plugin)
        {
            _plugin = plugin;

            var type = _plugin.GetType();

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

        public bool SupportsAutomaticScreenshots { get; set; } = false;
        public bool SupportsScreenshotSearch { get; set; } = false;

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

        public List<ScreenshotSearchResult> GetScreenshotSearchResult(Game game, string searchTerm)
        {
            if (!SupportsScreenshotSearch || _methodInfoGetScreenshotSearchResults == null)
            {
                return null;
            }

            var parametersArray = new object[] { game, searchTerm };

            return (List<ScreenshotSearchResult>)_methodInfoGetScreenshotSearchResults.Invoke(_plugin, parametersArray);
        }

        public async Task<bool> GetScreenshotsManualAsync(Game game, ScreenshotSearchResult searchResult)
        {
            if (!SupportsScreenshotSearch || _methodInfoGetScreenshotsManualAsync == null)
            {
                return false;
            }

            var parametersArray = new object[] { game, searchResult ?? _screenshotSearchResult };

            var resultTask = (Task<bool>)_methodInfoGetScreenshotsManualAsync.Invoke(_plugin, parametersArray);

            return await resultTask;
        }

        public List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            var result = new List<GenericItemOption>();

            result.AddRange(GetScreenshotSearchResult(_game, searchTerm));

            return result;
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
