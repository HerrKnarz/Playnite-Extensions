﻿using KNARZhelper;
using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to PCGamingWiki.
    /// </summary>
    internal class LinkPcGamingWiki : BaseClasses.Linker
    {
        public override string LinkName => "PCGamingWiki";
        public override string BaseUrl => "https://www.pcgamingwiki.com/wiki/";
        public override string SearchUrl => "https://www.pcgamingwiki.com/w/index.php?search={0}&fulltext=1";

        private readonly string _websiteUrl = "https://www.pcgamingwiki.com";

        // PCGamingWiki Links need the game with underscores instead of whitespaces and special characters simply encoded.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).CollapseWhitespaces()
                .Replace(" ", "_")
                .EscapeDataString();

        public override bool AddLink(Game game)
        {
            LinkUrl = string.Empty;

            if (LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            string gameName = GetGamePath(game, game.Name.RemoveEditionSuffix());

            if (!string.IsNullOrEmpty(gameName))
            {
                if (CheckLink($"{BaseUrl}{gameName}"))
                {
                    LinkUrl = $"{BaseUrl}{gameName}";
                }
                // if the first try didn't find a link, we try it with the capitalized game name.
                else
                {
                    string gameNameCapitalized = game.Name.CollapseWhitespaces().ToTitleCase().Replace(" ", "_").EscapeDataString();

                    if (gameNameCapitalized != gameName && CheckLink($"{BaseUrl}{gameNameCapitalized}"))
                    {
                        LinkUrl = $"{BaseUrl}{gameNameCapitalized}";
                    }
                }
            }

            return !string.IsNullOrEmpty(LinkUrl) && LinkHelper.AddLink(game, LinkName, LinkUrl);
        }

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new List<GenericItemOption>(ParseHelper.GetMediaWikiResultsFromHtml(SearchUrl, searchTerm, _websiteUrl, LinkName));
    }
}