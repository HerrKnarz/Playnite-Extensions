using LinkUtilities.LinkActions;
using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

internal class LinkGgDeals(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    public static string ClassId => $"linkutilities.ggdeals.link";
    public override string BaseUrl => "https://gg.deals/steam/app/";
    public override string BrowserSearchUrl => "https://gg.deals/games/?title=";
    public override bool CanBeSearched => false;
    public override string LinkName => "GG.deals";
    public override bool NeedsToBeChecked => false;
    public override int Priority => 10;

    // GG.deals only works with steam ids, since the website won't let us verify the links.
    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null) => SteamHelper.GetSteamId(game);
}