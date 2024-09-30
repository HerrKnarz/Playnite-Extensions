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
    ///     Adds a link to Adventure Gamers.
    /// </summary>
    internal class LinkAdventureGamers : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;

        public override string BaseUrl => "https://adventuregamers.com";
        public override string LinkName => "Adventure Gamers";
        public override string SearchUrl => "https://adventuregamers.com/games/search?keywords=";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                var web = new HtmlWeb();
                var doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                var htmlNodes =
                    doc.DocumentNode.SelectNodes("//div[@class='item_article']//h2//a[contains(@href,'games')]");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult
                    {
                        Name = WebUtility.HtmlDecode(n.InnerText),
                        Url = $"{BaseUrl}{n.GetAttributeValue("href", "")}",
                        Description = string.Empty
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