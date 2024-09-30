using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to GOG Database.
    /// </summary>
    internal class LinkGogDb : BaseClasses.Linker
    {
        private const string _baseUrl = "https://gogdb.org";
        public override string BaseUrl => _baseUrl + "/product/";
        public override string LinkName => "GOG Database";
        public override bool NeedsToBeChecked => false;

        // GOG Database Links need the gog game id. Because of that the add function only works with the gog library.
        public override string GetGamePath(Game game, string gameName = null) =>
            game.PluginId != Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e") ? string.Empty : game.GameId;

        //TODO: Maybe add a search function via GOG later.
    }
}