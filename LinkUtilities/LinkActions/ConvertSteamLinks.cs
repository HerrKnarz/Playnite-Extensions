using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

internal class ConvertSteamLinksArgs(string id, string name, IPlayniteApi api, List<BaseActionGame> games, string pluginName) : BaseActionArgs(id, name, api, games, pluginName)
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

    public override string Id => "linkutilities.convert.steam";
    public override string Name => Loc.action_name_convert_steam_links();

    public static ConvertSteamLinks Instance() => _instance ??= new ConvertSteamLinks();

    public override async Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args)
        => await ConvertAsync(game.Game, args);

    public override ConvertSteamLinksArgs GetActionArgs(IPlayniteApi api, List<BaseActionGame> games, string pluginName)
    {
        return new ConvertSteamLinksArgs(Id, Name, api, games, pluginName)
        {
            ProgressMessage = Loc.progress_converting_steam_links(),
            ResultMessageId = LocId.dialog_converted_steam_links_message
        };
    }

    private static async Task<bool> ConvertAsync(Game game, BaseActionArgs args)
        => args is ConvertSteamLinksArgs convertArgs && await SteamHelper.ConvertSteamLinksAsync(game, convertArgs.ToClient);
}