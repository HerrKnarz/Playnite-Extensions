﻿using KNARZhelper;
using LinkUtilities.Helper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to NEC Retro.
    /// </summary>
    internal class LinkNecRetro : BaseClasses.Linker
    {
        private const string _websiteUrl = "https://necretro.org";
        public override string BaseUrl => "https://necretro.org/";
        public override string BrowserSearchUrl => "https://necretro.org/index.php?search=";
        public override string CheckForContent => "itemtype=\"http://schema.org/VideoGame\"";
        public override string LinkName => "NEC Retro";
        public override string SearchUrl => "https://necretro.org/index.php?search={0}&fulltext=1";
        public override UrlLoadMethod UrlLoadMethod => UrlLoadMethod.OffscreenView;

        // NEC Retro Links need the game with underscores instead of whitespaces and special characters simply encoded.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).CollapseWhitespaces()
                .Replace(" ", "_")
                .EscapeDataString();

        public override List<GenericItemOption> GetSearchResults(string searchTerm)
            => new List<GenericItemOption>(ParseHelper.GetMediaWikiResultsFromHtml(SearchUrl, searchTerm, _websiteUrl, LinkName, 2));
    }
}