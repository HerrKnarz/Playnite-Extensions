using Playnite;

namespace LinkUtilities.Linker.LinkSources;

internal class LinkSteamDb : BaseClasses.Linker
{
    private const string _baseUrl = "https://steamdb.info";
    public override string BaseUrl => _baseUrl + "/app/";
    public override string LinkName => "SteamDB";
    public override bool NeedsToBeChecked => false;
    public override int Priority => 10;

    // SteamDB Links need the steam game id.
    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null) => GetSteamId(game);

    //LATER: Maybe add a search function via steam later.
}