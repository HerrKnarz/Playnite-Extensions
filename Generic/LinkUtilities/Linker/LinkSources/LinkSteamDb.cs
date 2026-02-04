using KNARZhelper.WebCommon;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker.LinkSources
{
    /// <summary>
    /// Adds a link to SteamDB.
    /// </summary>
    internal class LinkSteamDb : BaseClasses.Linker
    {
        private const string _baseUrl = "https://steamdb.info";
        public override string BaseUrl => _baseUrl + "/app/";
        public override string LinkName => "SteamDB";
        public override bool NeedsToBeChecked => false;
        public override int Priority => 10;
        public override UrlLoadMethod UrlLoadMethod => UrlLoadMethod.NewDefault;

        // SteamDb Links need the steam game id.
        public override string GetGamePath(Game game, string gameName = null) => GetSteamId(game);

        //TODO: Maybe add a search function via steam later.
    }
}