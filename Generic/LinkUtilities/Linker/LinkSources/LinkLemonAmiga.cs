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
    /// Adds a link to Lemon Amiga.
    /// </summary>
    internal class LinkLemonAmiga : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string BaseUrl => "https://www.lemonamiga.com/games/";
        public override string LinkName => "Lemon Amiga";
        public override string SearchUrl => "https://www.lemonamiga.com/games/list.php?list_title=";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                (var success, var document) = LoadDocument($"{SearchUrl}{searchTerm.UrlEncode()}");

                if (!success)
                {
                    return null;
                }

                var htmlNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'game-col')]/div/div[2]");

                if (htmlNodes?.Any() ?? false)
                {
                    var searchResults = new List<GenericItemOption>();

                    foreach (var node in htmlNodes)
                    {
                        var suffixNode = node.SelectSingleNode("./div[@class='game-grid-title']/a/img");
                        var suffix = string.Empty;

                        if (suffixNode != null)
                        {
                            suffix = $" ({suffixNode.GetAttributeValue("alt", "")})";
                        }

                        searchResults.Add(new SearchResult
                        {
                            Name = $"{WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='game-grid-title']/a").InnerText)}{suffix}",
                            Url = $"{BaseUrl}{node.SelectSingleNode("./div[@class='game-grid-title']/a").GetAttributeValue("href", "")}",
                            Description = $"{WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='grid-info']").InnerText.Trim())} {WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='grid-category']").InnerText.Trim())}"
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