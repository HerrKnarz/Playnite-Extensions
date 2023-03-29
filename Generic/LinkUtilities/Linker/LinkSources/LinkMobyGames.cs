using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to MobyGames.
    /// </summary>
    internal class LinkMobyGames : BaseClasses.Link
    {
        public override string LinkName { get; } = "MobyGames";
        public override string BaseUrl { get; } = "https://www.mobygames.com/game/";
        public override string SearchUrl { get; } = "https://www.mobygames.com/search/?type=game&q=";

        // MobyGames Links need the game name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//table/tr/td[last()]");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult()
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode("./b/a").InnerText),
                        Url = n.SelectSingleNode("./b/a").GetAttributeValue("href", ""),
                        Description = WebUtility.HtmlDecode(n.SelectSingleNode("./small[last()]").InnerText).CollapseWhitespaces()
                    }));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.GetSearchResults(searchTerm);
        }

        public LinkMobyGames() : base()
        {
        }
    }
}