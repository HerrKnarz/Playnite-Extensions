using KNARZhelper;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;
using WikipediaMetadata;
using WikipediaMetadata.Models;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Wikipedia.
    /// </summary>
    internal class LinkWikipedia : BaseClasses.Linker
    {
        public override string LinkName => "Wikipedia";
        public override string BaseUrl => "https://en.wikipedia.org/wiki/";
        public override string SearchUrl => "https://en.wikipedia.org/w/api.php?action=opensearch&format=xml&search={0}&limit=50";

        // Wikipedia Links need the game with underscores instead of whitespaces and special characters simply encoded.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).CollapseWhitespaces()
                .Replace(" ", "_")
                .EscapeDataString();

        public override bool AddSearchedLink(Game game, bool skipExistingLinks = false, bool cleanUpAfterAdding = true)
        {
            if (skipExistingLinks && LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            GenericItemOption result = GlobalSettings.Instance().OnlyATest
                ? GetSearchResults(game.Name)?.FirstOrDefault() ?? new WikipediaItemOption()
                : API.Instance.Dialogs.ChooseItemWithSearch(
                    new List<GenericItemOption>(),
                    GetSearchResults,
                    game.Name,
                    $"{ResourceProvider.GetString("LOCLinkUtilitiesDialogSearchGame")} ({LinkName})");

            return result != null && LinkHelper.AddLink(game, LinkName, BaseUrl + ((WikipediaItemOption)result).Key, false, cleanUpAfterAdding);
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new GameFinder(true).GetSearchResults(searchTerm);
    }
}