﻿using KNARZhelper;
using Playnite.SDK.Models;
using System.Threading;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to Map Genie.
    /// </summary>
    internal class LinkMapGenie : BaseClasses.Linker
    {
        public override string BaseUrl => "https://mapgenie.io/";
        public override string LinkName => "Map Genie";

        // Map Genie Links need the game name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
        {
            // sleep a while, so the site doesn't block us for too many requests. 
            // Todo: put the interval into the interface, so all links could use it, and put it in the loop to only use the delay, when adding several
            // links to the site in bulk.
            Thread.Sleep(200);

            return (gameName ?? game.Name).RemoveDiacritics()
                .RemoveSpecialChars()
                .Replace("_", " ")
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();
        }
    }
}