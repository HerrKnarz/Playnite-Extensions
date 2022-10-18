using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to the steam page of the game, if it is part of the steam library.
    /// </summary>
    class LibraryLinkSteam : LibraryLink
    {
        public override Guid LibraryId { get; } = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
        public override string LinkName { get; } = "Steam";
        public override string BaseUrl { get; } = "https://store.steampowered.com/app/{0}/";

        public override bool AddLink(Game game)
        {
            // Adding a link to steam is extremely simple. You only have to add the GameId to the base URL.
            LinkUrl = string.Format(BaseUrl, game.GameId);
            return LinkHelper.AddLink(game, LinkName, LinkUrl, Settings);
        }

        public LibraryLinkSteam(LinkUtilitiesSettings settings) : base(settings)
        {
        }
    }
}
