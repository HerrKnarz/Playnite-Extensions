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
    ///     Adds a link to Lemon Amiga.
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
                var web = new HtmlWeb();
                var doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                var htmlNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'game-col')]");

                if (htmlNodes?.Any() ?? false)
                {
                    return htmlNodes
                            .Select(node => new
                            {
                                node,
                                suffixNode = node.SelectSingleNode("./div/div[@class='game-grid-title']/a/img")
                            })
                            .Select(t => new
                            {
                                t,
                                suffix =
                                    t.suffixNode != null
                                        ? $" ({t.suffixNode.GetAttributeValue("alt", "")})"
                                        : string.Empty
                            })
                            .Select(t => new SearchResult
                            {
                                Name = $"{WebUtility.HtmlDecode(t.t.node.SelectSingleNode("./div/div[@class='game-grid-title']/a").InnerText)}{t.suffix}",
                                Url = $"{BaseUrl}{t.t.node.SelectSingleNode("./div/div[@class='game-grid-title']/a").GetAttributeValue("href", "")}",
                                Description = $"{WebUtility.HtmlDecode(t.t.node.SelectSingleNode("./div/div[@class='game-grid-info']").InnerText)}{WebUtility.HtmlDecode(t.t.node.SelectSingleNode("./div/div[@class='game-grid-category']").InnerText)}"
                            }).Cast<GenericItemOption>()
                        .ToList();
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