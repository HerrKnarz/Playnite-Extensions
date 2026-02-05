using KNARZhelper;
using KNARZhelper.WebCommon;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to Adventure Gamers.
    /// </summary>
    internal class LinkAdventureGamers : BaseClasses.Linker
    {
        public override string BaseUrl => "https://adventuregamers.com/games/";
        public override string CheckForContent => "<div class=\"game-detail-page\"";
        public override string LinkName => "Adventure Gamers";
        public override string SearchUrl => "https://adventuregamers.com/?s=";
        public override UrlLoadMethod UrlLoadMethod => UrlLoadMethod.NewDefault;

        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                var urlLoadResult = LinkWorker.LoadUrl($"{SearchUrl}{searchTerm.UrlEncode()}", DocumentType.Source, string.Empty, GlobalSettings.Instance().DebugMode);

                if (urlLoadResult.ErrorDetails.Length > 0 || urlLoadResult.Document is null)
                {
                    return null;
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
            }

            return base.GetSearchResults(searchTerm);
        }
    }
}