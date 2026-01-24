using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using Playnite.SDK;
using System.Collections.Generic;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to SNK Wiki.
    /// </summary>
    internal class LinkSnkWiki : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string BrowserSearchUrl => "https://snk.fandom.com/wiki/Special:Search?query=";
        public override string LinkName => "SNK Wiki";
        public override string SearchUrl => "https://snk.fandom.com/api.php?action=opensearch&format=xml&search={0}&limit=50";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new List<GenericItemOption>(ParseHelper.GetMediaWikiResultsFromApi(SearchUrl, searchTerm, LinkName));
    }
}