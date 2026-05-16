using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

internal class LinkGogDb(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    public static string ClassId => $"linkutilities.gogdb.link";
    public override string BaseUrl => "https://www.gogdb.org/product/";
    public override string LinkName => "GOG Database";
    public override bool NeedsToBeChecked => false;
    public override int Priority => 10;

    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
        => GogHelper.GetGogId(game);
}