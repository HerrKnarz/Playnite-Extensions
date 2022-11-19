using HtmlAgilityPack;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Arcade Database (arcadeitalia.net).
    /// </summary>
    class LinkArcadeDatabase : Link
    {
        public override string LinkName { get; } = "Arcade Database";
        public override string BaseUrl { get; } = "http://adb.arcadeitalia.net/dettaglio_mame.php?lang=en&game_name=";
        public override string SearchUrl { get; } = "http://adb.arcadeitalia.net/lista_mame.php?lang=en&ricerca=";

        internal string WebsiteUrl = "http://adb.arcadeitalia.net/";

        public override bool CheckLink(string link)
        {
            try
            {
                // Arcade Database returns code 200, if the game isn't found. So we have to check the HTML itself.
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(link);
                return doc.DocumentNode.SelectSingleNode("//div[@id='game_not_found']") == null;
            }
            catch
            {
                return false;
            }
        }

        public override string GetGamePath(Game game)
        {
            // Arcade Database needs the name of the game file, because it follows the MAME naming scheme.
            if (game.IsInstalled && game.Roms != null && game.Roms.Count > 0)
            {
                return Path.GetFileNameWithoutExtension(game.Roms[0].Path);
            }
            else
            {
                return string.Empty;
            }
        }

        public override List<GenericItemOption> SearchLink(string searchTerm)
        {
            SearchResults.Clear();

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load($"{SearchUrl}{searchTerm.UrlEncode()}");

                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//li[@class='elenco_galleria']");

                if (htmlNodes != null && htmlNodes.Count > 0)
                {
                    int counter = 0;

                    foreach (HtmlNode node in htmlNodes)
                    {
                        counter++;

                        SearchResults.Add(new SearchResult
                        {
                            Name = $"{counter}. {WebUtility.HtmlDecode(node.SelectSingleNode("./a/div[@class='titolo_galleria']").InnerText)}",
                            Url = $"{WebsiteUrl}{node.SelectSingleNode("./a").GetAttributeValue("href", "")}",
                            Description = $"{WebUtility.HtmlDecode(node.SelectSingleNode("./a/div[@class='romset_galleria']").InnerText)} - {WebUtility.HtmlDecode(node.SelectSingleNode("./a/div[@class='produttore_galleria']").InnerText)}"
                        }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {LinkName}");
            }

            return base.SearchLink(searchTerm);
        }

        public LinkArcadeDatabase(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}