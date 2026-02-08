using Playnite.SDK.Models;

namespace LinkUtilities.Linker.LinkSources
{
    internal class LinkSteamPeek : BaseClasses.Linker
    {
        private const string _baseUrl = "https://steampeek.hu";
        public override string BaseUrl => _baseUrl + "/?appid=";
        public override string LinkName => "SteamPeek";
        public override bool NeedsToBeChecked => false;
        public override int Priority => 10;

        // SteamPeek Links need the steam game id.
        public override string GetGamePath(Game game, string gameName = null) => GetSteamId(game);

        //TODO: Maybe add a search function via steam later.
    }
}