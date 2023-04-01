using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Arcade Database (arcadeitalia.net).
    /// </summary>
    internal class LinkArcadeDatabase : BaseClasses.Linker
    {
        private readonly string _websiteUrl = "http://adb.arcadeitalia.net/";

        public override string LinkName => "Arcade Database";
        public override string BaseUrl => "http://adb.arcadeitalia.net/dettaglio_mame.php?lang=en&game_name=";
        public override string SearchUrl => "http://adb.arcadeitalia.net/lista_mame.php?lang=en&ricerca=";

        public override bool CheckLink(string link)
        {
            try
            {
                // Arcade Database returns code 200, if the game isn't found. So we have to check the HTML itself.
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(link);
                return doc.DocumentNode.SelectSingleNode("//div[@id='game_not_found']") is null;
            }
            catch
            {
                return false;
            }
        }

        // Arcade Database needs the name of the game file, because it follows the MAME naming scheme.
        public override string GetGamePath(Game game, string gameName = null)
            => game.IsInstalled && (game.Roms?.Any() ?? false)
                ? Path.GetFileNameWithoutExtension(game.Roms[0].Path)
                : string.Empty;

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//li[@class='elenco_galleria']");

                if (htmlNodes?.Any() ?? false)
                {
                    return new List<GenericItemOption>(htmlNodes.Select(n => new SearchResult()
                    {
                        Name = WebUtility.HtmlDecode(n.SelectSingleNode("./a/div[@class='titolo_galleria']").InnerText),
                        Url = $"{_websiteUrl}{n.SelectSingleNode("./a").GetAttributeValue("href", "")}",
                        Description =
                            $"{WebUtility.HtmlDecode(n.SelectSingleNode("./a/div[@class='romset_galleria']").InnerText)} - {WebUtility.HtmlDecode(n.SelectSingleNode("./a/div[@class='produttore_galleria']").InnerText)}"
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