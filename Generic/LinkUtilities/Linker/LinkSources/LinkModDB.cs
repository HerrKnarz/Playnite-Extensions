using KNARZhelper;
using KNARZhelper.WebCommon;
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
    /// Adds a link to Mod DB.
    /// </summary>
    internal class LinkModDb : BaseClasses.Linker
    {
        private const string _websiteUrl = "https://www.moddb.com";
        public override string BaseUrl => _websiteUrl + "/games/";
        public override string CheckForContent => "<h2 itemprop=\"name\"";
        public override string LinkName => "Mod DB";
        public override string SearchUrl => _websiteUrl + "/games?filter=t&kw=";
        public override UrlLoadMethod UrlLoadMethod => UrlLoadMethod.NewDefault;

        // Mod DB Links need the game name in lowercase without special characters and hyphens
        // instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                (var success, var document) = LoadDocument($"{SearchUrl}{searchTerm.UrlEncode()}", "<div class=\"col-12 result\"", true);

                if (!success)
                {
                    return null;
                }

                var htmlNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'rowcontent')]/div[@class='content']");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode("./h4/a")?.InnerText ?? "unknown"),
                        Url = $"{_websiteUrl}{n.SelectSingleNode("./h4/a")?.GetAttributeValue("href", "")}",
                        Description = WebUtility.HtmlDecode(n.SelectSingleNode("./span[@class='subheading']")?.InnerText.CollapseWhitespaces() ?? string.Empty)
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