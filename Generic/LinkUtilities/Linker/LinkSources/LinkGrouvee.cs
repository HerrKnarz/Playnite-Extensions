using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Interfaces;
using LinkUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
// ReSharper disable IdentifierTypo

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to Grouvee.
    /// </summary>
    internal class LinkGrouvee : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;

        public override string BaseUrl => "https://www.grouvee.com";
        public override string LinkName => "Grouvee";
        public override string SearchUrl => "https://www.grouvee.com/search/?q=";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                var web = new HtmlWeb();
                var doc =
                    web.Load(
                        $"{SearchUrl}{searchTerm.UrlEncode()}");

                var htmlNodes =
                    doc.DocumentNode.SelectNodes("//div[@class='details-section']");

                if (htmlNodes?.Any() ?? false)
                {
                    var searchResults = new List<GenericItemOption>();

                    foreach (var node in htmlNodes)
                    {
                        foreach (var span in node.SelectNodes("./h4/span"))
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