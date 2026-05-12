using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

public class LinkSteamDb(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private const string _baseUrl = "https://steamdb.info";
    public static string ClassId => $"linkutilities.steamdb.link";
    public override string BaseUrl => _baseUrl + "/app/";
    public override string LinkName => "SteamDB";
    public override bool NeedsToBeChecked => false;
    public override int Priority => 10;

    // SteamDB Links need the steam game id.
    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null) => SteamHelper.GetSteamId(game);

    //LATER: Maybe add a search function via steam later.
}