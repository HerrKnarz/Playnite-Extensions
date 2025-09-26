using LinkUtilities.Helper;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    ///     Adds a link to ProtonDB.
    /// </summary>
    internal class LinkProtonDb : BaseClasses.Linker
    {
        private const string _baseUrl = "https://www.protondb.com";
        public override string BaseUrl => _baseUrl + "/app/";
        public override string LinkName => "ProtonDB";

        // ProtonDb Links need the steam game id.
        public override string GetGamePath(Game game, string gameName = null) => SteamHelper.GetSteamId(game);

        //TODO: Maybe add a search function via steam later.
    }
}