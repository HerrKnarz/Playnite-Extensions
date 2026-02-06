using KNARZhelper;
using KNARZhelper.WebCommon;
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
    /// Adds a link to Grouvee.
    /// </summary>
    internal class LinkGrouvee : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;

        public override string BaseUrl => "https://www.grouvee.com";
        public override string LinkName => "Grouvee";
        public override string SearchUrl => "https://www.grouvee.com/search/?q=";
        public override UrlLoadMethod UrlLoadMethod => UrlLoadMethod.NewDefault;

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                (var success, var document) = LoadDocument($"{SearchUrl}{searchTerm.UrlEncode()}");

                if (!success)
                {
                    return null;
                }

                var htmlNodes = document.DocumentNode.SelectNodes("//div[@class='game-info']");

                if (htmlNodes?.Any() ?? false)
                {
                    var searchResults = new List<GenericItemOption>();
                    var description = string.Empty;

                    foreach (var node in htmlNodes)
                    {
                        var platformNodes = node.SelectNodes("./div[@class='small platform-list']/span");

                        if (platformNodes?.Any() ?? false)
                        {
                            description = string.Join(", ", platformNodes.Select(p => WebUtility.HtmlDecode(p.InnerText.Trim())));
                        }

                        searchResults.Add(new SearchResult
                        {
                            Name = WebUtility.HtmlDecode(node.SelectSingleNode("./h6/a").InnerText.Trim()),
                            Url = $"{BaseUrl}{node.SelectSingleNode("./h6/a").GetAttributeValue("href", "")}",
                            Description = description
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