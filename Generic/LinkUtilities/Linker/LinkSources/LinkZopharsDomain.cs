using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Zophar's Domain for game soundtracks.
    /// </summary>
    internal class LinkZopharsDomain : Link
    {
        public override string LinkName { get; } = "Zophar (Music)";
        public override LinkAddTypes AddType { get; } = LinkAddTypes.SingleSearchResult;
        public override string SearchUrl { get; } = "https://www.zophar.net/music/search?search=";
        public override string BaseUrl { get; } = "https://www.zophar.net";

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'regularrow')]");

                if (htmlNodes != null && htmlNodes.Count > 0)
                {
                    foreach (HtmlNode node in htmlNodes)
                    {
                        SearchResults.Add(new SearchResult
                        {
                            Name = WebUtility.HtmlDecode(node.SelectSingleNode("./td[@class='name']").InnerText),
                            Url = $"{BaseUrl}{node.SelectSingleNode("./td[@class='name']/a").GetAttributeValue("href", "")}",
                            Description = WebUtility.HtmlDecode(node.SelectSingleNode("./td[@class='console']").InnerText)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.SearchLink(searchTerm);
        }

        public LinkZopharsDomain(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}