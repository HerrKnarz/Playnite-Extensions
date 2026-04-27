using LinkUtilities.LinkActions;
using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

internal class LinkProtonDb(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private const string _baseUrl = "https://www.protondb.com";
    public static string ClassId => $"linkutilities.protondb.link";
    public override string BaseUrl => _baseUrl + "/app/";
    public override string LinkName => "ProtonDB";
    public override int Priority => 10;

    // ProtonDB Links need the steam game id.
    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null) => SteamHelper.GetSteamId(game);

    //LATER: Maybe add a search function via steam later.
}