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
    internal class LinkFamilyGamingDatabase : BaseClasses.Linker
    {
        private const string _websiteUrl = "https://www.familygamingdatabase.com";
        public override string BaseUrl => _websiteUrl + "/en-gb/game/";
        public override string CheckForContent => "<div class=\"gameTitleShare\"";
        public override int Delay => 200;
        public override string LinkName => "Family Gaming Database";
        public override string SearchUrl => _websiteUrl + "/search/text/";

        // Family Gaming Database Links need the game name in lowercase without special characters
        // and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace(" ", "+");

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                (var success, var document) = LoadDocument($"{SearchUrl}{searchTerm.UrlEncode()}");

                if (!success)
                {
                    return null;
                }

                var htmlNodes = document.DocumentNode.SelectSingleNode("//div[@id='searchResultsFlexContainer']").SelectNodes(".//div[@class='listViewOverview']/a[@class='quietLink']");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult
                    {
                        Name = WebUtility.HtmlDecode(n.FirstChild.FirstChild.InnerText),
                        Url = n.GetAttributeValue("href", ""),
                        Description = WebUtility.HtmlDecode(n.FirstChild.InnerText)
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