using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to GG.deals.
    /// </summary>
    internal class LinkGgDeals : BaseClasses.Linker
    {
        public override string BaseUrl => "https://gg.deals/steam/app/";
        public override string BrowserSearchUrl => "https://gg.deals/games/?title=";
        public override bool CanBeSearched => false;
        public override string LinkName => "GG.deals";
        public override bool NeedsToBeChecked => false;

        // GG.deals only works with steam ids, since the website won't let us verify the links.
        public override string GetGamePath(Game game, string gameName = null) =>
            game.PluginId != Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab") ? string.Empty : game.GameId;
    }
}