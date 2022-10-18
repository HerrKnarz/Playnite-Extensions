using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to the steam page of the game, if it is part of the steam library.
    /// </summary>
    class LibraryLinkSteam : LibraryLink
    {
        public override bool AddLink(Game game)
        {
            // Adding a link to steam is extremely simple. You only have to add the GameId to the base URL.
            return LinkHelper.AddLink(game, LinkName, string.Format(LinkUrl, game.GameId), Settings);
        }

        public LibraryLinkSteam(LinkUtilitiesSettings settings)
        {
            LibraryId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
            LinkName = "Steam";
            LinkUrl = "https://store.steampowered.com/app/{0}/";
            Settings = settings;
        }
    }
}
