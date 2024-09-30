using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Interfaces;
using LinkUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to Zophar's Domain for game soundtracks.
    /// </summary>
    internal class LinkZopharsDomain : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string BaseUrl => "https://www.zophar.net";
        public override string LinkName => "Zophar (Music)";
        public override string SearchUrl => "https://www.zophar.net/music/search?search=";

        public override string GetBrowserSearchLink(string searchTerm) => $"{BrowserSearchUrl}{searchTerm.RemoveDiacritics().EscapeDataString()}";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                var web = new HtmlWeb();
                var doc = web.Load(GetBrowserSearchLink(searchTerm));

                var htmlNodes = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'regularrow')]");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode("./td[@class='name']").InnerText),
                        Url = $"{BaseUrl}{n.SelectSingleNode("./td[@class='name']/a").GetAttributeValue("href", "")}",
                        Description = WebUtility.HtmlDecode(n.SelectSingleNode("./td[@class='console']").InnerText)
                    }));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.GetSearchResults(searchTerm);
        }
    }
}