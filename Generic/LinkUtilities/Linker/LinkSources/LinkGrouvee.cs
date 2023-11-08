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
    ///     Adds a link to Grouvee.
    /// </summary>
    internal class LinkGrouvee : BaseClasses.Linker
    {
        public override string LinkName => "Grouvee";
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string SearchUrl => "https://www.grouvee.com/search/?q=";

        public override string BaseUrl => "https://www.grouvee.com";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc =
                    web.Load(
                        $"{SearchUrl}{searchTerm.UrlEncode()}");

                HtmlNodeCollection htmlNodes =
                    doc.DocumentNode.SelectNodes("//div[@class='details-section']");

                if (htmlNodes?.Any() ?? false)
                {
                    List<GenericItemOption> searchResults = new List<GenericItemOption>();

                    foreach (HtmlNode node in htmlNodes)
                    {
                        foreach (HtmlNode span in node.SelectNodes("./h4/span"))
                        {
                            span.Remove();
                        }

                        searchResults.Add(new SearchResult
                        {
                            Name = WebUtility.HtmlDecode(node.SelectSingleNode("./h4[@class='media-heading']").InnerText.Trim()),
                            Url = $"{BaseUrl}{node.SelectSingleNode("./h4[@class='media-heading']/a").GetAttributeValue("href", "")}",
                            Description = WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='wrapper']").InnerText.Trim())
                        });
                    }

                    return searchResults;
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