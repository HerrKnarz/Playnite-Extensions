using LinkUtilities.Helper;
using Playnite;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

internal class AddLinkFromClipboard : BaseAction
{
    private static AddLinkFromClipboard? _instance;

    private AddLinkFromClipboard()
    { }

    public override string Id => "linkutilities.clipboard.link";

    /// <summary>
    /// Name of the link to be added in the "AddLink" action
    /// </summary>
    public string? LinkName { get; set; }

    /// <summary>
    /// URL of the link to be added in the "AddLink" action
    /// </summary>
    public string? LinkUrl { get; set; }

    public override string Name => "Clipboard link";

    public static AddLinkFromClipboard Instance() => _instance ??= new AddLinkFromClipboard();

    public override async Task<bool> ExecuteAsync(GameEx game, BaseActionArgs args)
        => await LinkHelper.AddLinkAsync(game.Game, LinkName, LinkUrl, null, false);

    public override BaseActionArgs GetActionArgs(IPlayniteApi api, List<GameEx> games, string pluginName)
    {
        var args = base.GetActionArgs(api, games, pluginName);

        args.ProgressMessage = Loc.progress_adding_website_links();
        args.ResultMessageId = LocId.dialog_added_links_message;

        return args;
    }

    public override async Task<bool> PrepareAsync(BaseActionArgs args)
    {
        LinkName = string.Empty;
        LinkUrl = string.Empty;

        var url = Clipboard.GetText();
        var tempLinkName = string.Empty;

        if (url.Length == 0 || !Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return false;
        }

        LinkUrl = url;

        if (LinkUtilitiesPlugin.Settings.LinkNamePatterns.LinkMatch(ref tempLinkName, url, true))
        {
            LinkName = tempLinkName;
            return true;
        }

        // NEXT: Implement special dialog with option to choose link type or add a new one.
        var selectResult = await args.Api.Dialogs.SelectStringAsync(
            Loc.dialog_enter_link_name() + Environment.NewLine + url,
            Loc.dialog_name_link_caption(),
            tempLinkName);

        if (!selectResult.Result)
        {
            return false;
        }

        LinkName = selectResult.SelectedString;

        return true;
    }
}