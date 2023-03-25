using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Adventure Gamers.
    /// </summary>
    internal class LinkAdventureGamers : BaseClasses.Link
    {
        public override string LinkName { get; } = "Adventure Gamers";
        public override LinkAddTypes AddType { get; } = LinkAddTypes.SingleSearchResult;
        public override string SearchUrl { get; } = "https://adventuregamers.com/games/search?keywords=";

        public override string BaseUrl { get; } = "https://adventuregamers.com";

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.RemoveSpecialChars().CollapseWhitespaces().Replace(" ", "+").ToLower()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//div[@class='item_article']//h2//a[contains(@href,'games')]");

                if (htmlNodes != null && htmlNodes.Count > 0)
                {
                    foreach (HtmlNode node in htmlNodes)
                    {
                        SearchResults.Add(new SearchResult
                        {
                            Name = WebUtility.HtmlDecode(node.InnerText),
                            Url = $"{BaseUrl}{node.GetAttributeValue("href", "")}",
                            Description = string.Empty
                        }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.SearchLink(searchTerm);
        }

        public LinkAdventureGamers() : base()
        {
        }
    }
}