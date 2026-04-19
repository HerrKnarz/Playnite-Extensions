using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

internal class ConvertSteamLinksArgs(IPlayniteApi api) : BaseActionArgs(api)
{
    public bool ToClient { get; set; } = true;
}

/// <summary>
/// Class to change steam links between https and app-urls.
/// </summary>
internal class ConvertSteamLinks : BaseAction
{
    private static ConvertSteamLinks? _instance;

    private ConvertSteamLinks()
    { }

    public override string ProgressMessage => Loc.progress_converting_steam_links();

    public override string ResultMessageId => LocId.dialog_converted_steam_links_message;

    public static ConvertSteamLinks Instance() => _instance ??= new ConvertSteamLinks();

    public override async Task<bool> ExecuteAsync(GameEx game, BaseActionArgs args)
    {
        if (await ConvertAsync(game.Game, args))
        {
            _gamesAffected.Add(game.Game);

            return true;
        }

        return false;
    }

    private static async Task<bool> ConvertAsync(Game game, BaseActionArgs args)
        => args is ConvertSteamLinksArgs convertArgs && await SteamHelper.ConvertSteamLinksAsync(game, convertArgs.ToClient);
}