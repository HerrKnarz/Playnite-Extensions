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
    ///     Adds a link to Co-Optimus.
    /// </summary>
    internal class LinkCoOptimus : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string BrowserSearchUrl => "https://www.co-optimus.com/search.php?q=";
        public override string LinkName => "Co-Optimus";
        public override string SearchUrl => "https://api.co-optimus.com/games.php?search=true&name=";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                var web = new HtmlWeb();
                var doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                var htmlNodes = doc.DocumentNode.SelectNodes("//game");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode("./title").InnerText),
                        Url = n.SelectSingleNode("./url").InnerText,
                        Description = $"{WebUtility.HtmlDecode(n.SelectSingleNode("./system").InnerText)} - {WebUtility.HtmlDecode(n.SelectSingleNode("./publisher").InnerText)} ({n.SelectSingleNode("./releasedate").InnerText})"
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