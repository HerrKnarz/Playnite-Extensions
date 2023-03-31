using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Zophar's Domain for game soundtracks.
    /// </summary>
    internal class LinkZopharsDomain : BaseClasses.Linker
    {
        public override string LinkName { get; } = "Zophar (Music)";
        public override LinkAddTypes AddType { get; } = LinkAddTypes.SingleSearchResult;
        public override string SearchUrl { get; } = "https://www.zophar.net/music/search?search=";
        public override string BaseUrl { get; } = "https://www.zophar.net";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'regularrow')]");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult()
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