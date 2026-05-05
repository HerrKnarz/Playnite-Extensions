using LinkUtilities.LinkActions;
using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

public class LinkSteamPeek(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private const string _baseUrl = "https://steampeek.hu";
    public static string ClassId => $"linkutilities.steampeek.link";
    public override string BaseUrl => _baseUrl + "/?appid=";
    public override string LinkName => "SteamPeek";
    public override bool NeedsToBeChecked => false;
    public override int Priority => 10;

    // SteamPeek Links need the steam game id.
    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null) => SteamHelper.GetSteamId(game);

    //LATER: Maybe add a search function via steam later.
}