using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    ///     Adds a link to ProtonDB.
    /// </summary>
    internal class LinkProtonDb : BaseClasses.Linker
    {
        private const string _baseUrl = "https://www.protondb.com";
        public override string LinkName => "ProtonDB";
        public override string BaseUrl => _baseUrl + "/app/";

        // ProtonDb Links need the steam game id. Because of that the add function only works with the steam library.
        public override string GetGamePath(Game game, string gameName = null) =>
            game.PluginId != Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab") ? string.Empty : game.GameId;

        //TODO: Maybe add a search function via steam later.
    }
}