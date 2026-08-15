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
    internal class LinkUVL : BaseClasses.Linker
    {
        private const string _websiteUrl = "https://www.uvlist.net";
        public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
        public override string LinkName => "UVL";
        public override string SearchUrl => $"{_websiteUrl}/globalsearch/?t=";

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            // TODO: Support platforms!
            try
            {
                (var success, var document) = LoadDocument($"{SearchUrl}{searchTerm.UrlEncode()}", string.Empty, true);

                if (!success)
                {
                    return null;
                }

                var cards = document.DocumentNode.SelectNodes("//div[contains(@class, 'card')]");

                if (cards == null)
                {
                    return null;
                }

                HtmlNodeCollection results = null;

                foreach (var card in cards)
                {
                    if (card.SelectSingleNode(".//h2[@class='card-header']")?.InnerText == "Games by title")
                    {
                        results = card.SelectNodes(".//tbody/tr");

                        break;
                    }
                }

                if (results?.Any() ?? false)
                {
                    return new List<GenericItemOption>(results.Select(n => new SearchResult
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode("./td[1]/a")?.InnerText),
                        Url = _websiteUrl + n.SelectSingleNode("./td[1]/a")?.GetAttributeValue("href", ""),
                        Description = WebUtility.HtmlDecode($"{n.SelectSingleNode("./td[2]")?.InnerText} / {n.SelectSingleNode("./td[3]/span")?.InnerText} / {n.SelectSingleNode("./td[4]")?.InnerText}")
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
