using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    ///     Adds a link to SteamDB.
    /// </summary>
    internal class LinkSteamDb : BaseClasses.Linker
    {
        private const string _baseUrl = "https://steamdb.info";
        public override string LinkName => "SteamDB";
        public override string BaseUrl => _baseUrl + "/app/";
        public override bool NeedsToBeChecked => false;

        // SteamDb Links need the steam game id. Because of that the add function only works with the steam library.
        public override string GetGamePath(Game game, string gameName = null) =>
            game.PluginId != Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab") ? string.Empty : game.GameId;

        //TODO: Maybe add a search function via steam later.
    }
}