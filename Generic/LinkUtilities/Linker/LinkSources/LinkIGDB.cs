using KNARZhelper;
using LinkUtilities.Helper;
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
    ///     Adds a link to Mod DB.
    /// </summary>
    internal class LinkIGDB : BaseClasses.Linker
    {
        private string _gameSlug = string.Empty;

        private const string _websiteUrl = "https://www.igdb.com";
        public override string BaseUrl => _websiteUrl + "/games/";
        public override string CheckForContent => $"<meta content=\"{BaseUrl}{_gameSlug}\"";
        public override string LinkName => "IGDB";
        public override string SearchUrl => _websiteUrl + "/search?utf8=✓&q=";
        public override UrlLoadMethod UrlLoadMethod => UrlLoadMethod.OffscreenView;

        //TODO: Find a reliable way to verify the correct game page. For some reason it always returns the same url and also has the game-slug-id, although it doesn't anymore when opening it in browser...

        // IGDB Links need the game name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
        {
            _gameSlug = (gameName ?? game.Name).RemoveDiacritics()
                        .RemoveSpecialChars()
                        .CollapseWhitespaces()
                        .Replace(" ", "-")
                        .ToLower();

            return _gameSlug;
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                var urlLoadResult = LinkHelper.LoadHtmlDocument($"{SearchUrl}{searchTerm.UrlEncode()}", UrlLoadMethod.OffscreenView);

                if (urlLoadResult.ErrorDetails.Length > 0 || urlLoadResult.Document is null)
                {
                    return null;
                }

                var htmlNodes = urlLoadResult.Document.DocumentNode.SelectNodes("//div[@id='search-results']//div[@class='media-body']");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode("./a")?.InnerText ?? "unknown"),
                        Url = $"{_websiteUrl}{n.SelectSingleNode("./a")?.GetAttributeValue("href", "")}",
                        Description = WebUtility.HtmlDecode(n.SelectSingleNode("./div[@class='mar-md-bottom']/a")?.InnerText.CollapseWhitespaces() ?? string.Empty)
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