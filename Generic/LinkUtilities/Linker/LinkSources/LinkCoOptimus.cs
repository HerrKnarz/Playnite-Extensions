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
    /// Adds a link to Co-Optimus.
    /// </summary>
    internal class LinkCoOptimus : BaseClasses.Link
    {
        public override string LinkName { get; } = "Co-Optimus";
        public override LinkAddTypes AddType { get; } = LinkAddTypes.SingleSearchResult;
        public override string SearchUrl { get; } = "https://api.co-optimus.com/games.php?search=true&name=";

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//game");

                if (htmlNodes?.Any() ?? false)
                {
                    foreach (HtmlNode node in htmlNodes)
                    {
                        SearchResults.Add(new SearchResult
                        {
                            Name = WebUtility.HtmlDecode(node.SelectSingleNode("./title").InnerText),
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

        public LinkCoOptimus() : base()
        {
        }
    }
}