using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using Playnite.SDK;
using System.Collections.Generic;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to Nintendo Wiki.
    /// </summary>
    internal class LinkNintendoWiki : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string BaseUrl => "https://nintendo.fandom.com/wiki/";
        public override string BrowserSearchUrl => "https://nintendo.fandom.com/wiki/Special:Search?query=";
        public override string LinkName => "Nintendo Wiki";
        public override string SearchUrl => "https://nintendo.fandom.com/api.php?action=opensearch&format=xml&search={0}&limit=50";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new List<GenericItemOption>(ParseHelper.GetMediaWikiResultsFromApi(SearchUrl, searchTerm, LinkName));
    }
}