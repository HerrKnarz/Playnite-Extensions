using Playnite;

namespace LinkUtilities.Linker.LinkSources;

internal class LinkProtonDb : BaseClasses.Linker
{
    private const string _baseUrl = "https://www.protondb.com";
    public override string BaseUrl => _baseUrl + "/app/";
    public override string LinkName => "ProtonDB";
    public override int Priority => 10;

    // ProtonDB Links need the steam game id.
    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null) => GetSteamId(game);

    //LATER: Maybe add a search function via steam later.
}