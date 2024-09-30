using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to Hardcore Gaming 101.
    /// </summary>
    internal class LinkHg101 : BaseClasses.Linker
    {
        public override string BaseUrl => "http://www.hardcoregaming101.net/";
        public override string LinkName => "Hardcore Gaming 101";
        public override string SearchUrl => "http://www.hardcoregaming101.net/?s=";

        // HG101 Links need the game name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                var doc = new HtmlWeb().Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                var htmlNodes = doc.DocumentNode.SelectNodes("//header[@class='entry-header']");

                if (htmlNodes?.Any() ?? false)
                {
                    return htmlNodes
                        .Select(node => new
                        {
                            node,
                            reviewNodes = node.SelectNodes("./div[@class='index-entry-meta']/div[a='Review']")
                        })
                        .Where(t => t.reviewNodes?.Any() ?? false)
                        .Select(t => new SearchResult
                        {
                            Name = WebUtility.HtmlDecode(t.node.SelectSingleNode("./h2/a").InnerText),
                            Url = t.node.SelectSingleNode("./h2/a").GetAttributeValue("href", ""),
                            Description =
                                WebUtility.HtmlDecode(t.node.SelectNodes("./div[@class='index-entry-meta']/div/a")
                                    .Select(tagNode => tagNode.InnerText)
                                    .Aggregate((total, part) => total + ", " + part))
                        }).Cast<GenericItemOption>().ToList();
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