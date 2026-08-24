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
    public class LinkHallOfLight : BaseClasses.Linker
    {
        private const string _websiteUrl = "https://amiga.abime.net";
        public override string BaseUrl => $"{_websiteUrl}/games/view/";
        public override string LinkName => "Hall Of Light";
        public override string SearchUrl => $"{_websiteUrl}/games/list/?gamename=";

        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name)
                .SpecialCharsToWords()
                .RemoveSpecialChars()
                .Replace("-", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                (var success, var document) = LoadDocument($"{SearchUrl}{searchTerm.UrlEncode()}");

                if (!success)
                {
                    return null;
                }

                var htmlNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'game-grid')]/div/div");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode("./div[contains(@class, 'gamecolumn_name')]/a/h4").InnerText),
                        Url = _websiteUrl + WebUtility.HtmlDecode(n.SelectSingleNode("./div[contains(@class, 'gamecolumn_name')]/a").GetAttributeValue("href", "")),
                        Description = $"{WebUtility.HtmlDecode(n.SelectSingleNode("./div[contains(@class, 'gamecolumn_hardware')]")?.InnerText)} - {WebUtility.HtmlDecode(n.SelectSingleNode("./div[contains(@class, 'gamecolumn_year')]")?.InnerText)}"
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
