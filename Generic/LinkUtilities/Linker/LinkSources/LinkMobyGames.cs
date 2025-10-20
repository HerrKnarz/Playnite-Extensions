using KNARZhelper;
using LinkUtilities.Helper;
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
    ///     Adds a link to MobyGames.
    /// </summary>
    internal class LinkMobyGames : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string LinkName => "MobyGames";
        public override string SearchUrl => "https://www.mobygames.com/search/?type=game&q=";
        public override UrlLoadMethod UrlLoadMethod => UrlLoadMethod.OffscreenView;

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                var urlLoadResult = LinkHelper.LoadHtmlDocument($"{SearchUrl}{searchTerm.UrlEncode()}", UrlLoadMethod.OffscreenView);

                if (urlLoadResult.ErrorDetails.Length > 0 || urlLoadResult.Document is null)
                {
                    return null;
                }

                var htmlNodes = urlLoadResult.Document.DocumentNode.SelectNodes("//tbody/tr/td[last()]");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode(".//b/a").InnerText),
                        Url = n.SelectSingleNode(".//b/a").GetAttributeValue("href", ""),
                        Description = WebUtility.HtmlDecode(n.SelectSingleNode(".//small[last()]").InnerText).CollapseWhitespaces()
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