using HtmlAgilityPack;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Co-Optimus.
    /// </summary>
    class LinkCoOptimus : Link
    {
        public override string LinkName { get; } = "Co-Optimus";
        public override string BaseUrl { get; } = string.Empty;
        public override string SearchUrl { get; } = "https://api.co-optimus.com/games.php?search=true&name=";

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//game");

                if (htmlNodes != null && htmlNodes.Count > 0)
                {
                    int counter = 0;
                    foreach (HtmlNode node in htmlNodes)
                    {
                        counter++;

                        SearchResults.Add(new SearchResult
                        {
                            Name = $"{counter}. {WebUtility.HtmlDecode(node.SelectSingleNode("./title").InnerText)}",
                            Url = node.SelectSingleNode("./url").InnerText,
                            Description = $"{WebUtility.HtmlDecode(node.SelectSingleNode("./system").InnerText)} - {WebUtility.HtmlDecode(node.SelectSingleNode("./publisher").InnerText)} ({node.SelectSingleNode("./releasedate").InnerText})"
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

        public LinkCoOptimus(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}