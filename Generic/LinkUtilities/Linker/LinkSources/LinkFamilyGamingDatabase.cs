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
                    var searchResults = new List<GenericItemOption>();

                    foreach (var node in htmlNodes)
                    {
                        if (!node.HasChildNodes)
                        {
                            continue;
                        }

                        var name = WebUtility.HtmlDecode(node.FirstChild.FirstChild.InnerText);
                        var url = node.GetAttributeValue("href", "");
                        var description = WebUtility.HtmlDecode(node.FirstChild.InnerText);

                        searchResults.Add(new SearchResult
                        {
                            Name = name,
                            Url = url,
                            Description = description
                        });
                    }

                    return searchResults;
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