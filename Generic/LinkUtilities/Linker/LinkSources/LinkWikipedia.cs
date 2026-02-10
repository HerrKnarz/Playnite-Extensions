using LinkUtilities.Helper;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;
using WikipediaMetadata;
using WikipediaMetadata.Models;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to Wikipedia.
    /// </summary>
    internal class LinkWikipedia : BaseClasses.Linker
    {
        public override string BaseUrl => "https://en.wikipedia.org/wiki/";
        public override string BrowserSearchUrl => "https://en.wikipedia.org/w/index.php?search=";
        public override string LinkName => "Wikipedia";
        public override string SearchUrl => "https://en.wikipedia.org/w/api.php?action=opensearch&format=xml&search={0}&limit=50";

        public override bool AddSearchedLink(Game game, bool skipExistingLinks = false, bool cleanUpAfterAdding = true)
        {
            if (skipExistingLinks && LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            var result = GlobalSettings.Instance().OnlyATest
                ? GetSearchResults(game.Name)?.FirstOrDefault() ?? new WikipediaItemOption()
                : API.Instance.Dialogs.ChooseItemWithSearch(
                    new List<GenericItemOption>(),
                    GetSearchResults,
                    game.Name,
                    $"{ResourceProvider.GetString("LOCLinkUtilitiesDialogSearchGame")} ({LinkName})");

            return result != null && LinkHelper.AddLink(game, LinkName, BaseUrl + ((WikipediaItemOption)result).Key, false, cleanUpAfterAdding);
        }

        public override bool FindLinks(Game game, out List<Link> links)
        {
            links = new List<Link>();

            var page = new GameFinder(true).FindGame(game.Name);

            if (page == null)
            {
                return false;
            }

            links.Add(new Link(LinkName, BaseUrl + page.Key));

            return links.Count > 0;
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new GameFinder(true).GetSearchResults(searchTerm);
    }
}