using KNARZhelper;
using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
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
    ///     Adds a link to Adventure Gamers.
    /// </summary>
    internal class LinkAdventureGamers : BaseClasses.Linker
    {
        public override LinkAddTypes AddType => LinkAddTypes.UrlMatch;
        public override string BaseUrl => "https://adventuregamers.com/games/";
        public override string LinkName => "Adventure Gamers";
        public override string SearchUrl => "https://adventuregamers.com/?s=";

        public override UrlLoadMethod UrlLoadMethod => UrlLoadMethod.LoadFromBrowser;

        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            var errorResult = new List<GenericItemOption>
            {
                new SearchResult
                {
                    Name = $"Error loading data from {LinkName}",
                    Url = string.Empty,
                    Description = string.Empty
                }
            };

            try
            {
                var urlLoadResult = LinkHelper.LoadHtmlDocument($"{SearchUrl}{searchTerm.UrlEncode()}", UrlLoadMethod.LoadFromBrowser);

                if (urlLoadResult.ErrorDetails.Any())
                {
                    errorResult[0].Description = urlLoadResult.ErrorDetails;

                    return errorResult;
                }

                if (urlLoadResult.Document is null)
                {
                    return errorResult;
                }

                var htmlNodes = urlLoadResult.Document.DocumentNode.SelectSingleNode("//div[@id='search-adgames']").SelectNodes(".//a[@class='search-title-text']");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult
                    {
                        Name = WebUtility.HtmlDecode(n.InnerText),
                        Url = n.GetAttributeValue("href", ""),
                        Description = string.Empty
                    }));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");

                errorResult[0].Description = ex.Message;

                return errorResult;
            }

            return base.GetSearchResults(searchTerm);
        }
    }
}