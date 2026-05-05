using LinkUtilities.Helper;
using Playnite;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

public class AddLinkFromClipboard : BaseAction
{
    public AddLinkFromClipboard()
    { }

    public override string Id => ActionIds.TypeAddFromClipboard;

    /// <summary>
    /// Name of the link to be added in the "AddLink" action
    /// </summary>
    public string? LinkName { get; set; }

    /// <summary>
    /// URL of the link to be added in the "AddLink" action
    /// </summary>
    public string? LinkUrl { get; set; }

    public override string Name => Loc.action_name_clipboard_links();

    public static async Task CreateAndExecuteAsync(IPlayniteApi api, List<BaseActionGame> games, string pluginName)
    {
        var action = new AddLinkFromClipboard();
        await action.DoForAllAsync(action.GetActionArgs(api, games, pluginName));
    }

    public override async Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args)
        => await LinkHelper.AddLinkAsync(game.Game, LinkName, LinkUrl, null, false);

    public override BaseActionArgs GetActionArgs(IPlayniteApi api, List<BaseActionGame> games, string pluginName)
    {
        var args = base.GetActionArgs(api, games, pluginName);

        args.ProgressMessage = Loc.progress_adding_website_links();
        args.ResultMessageId = LocId.dialog_added_links_message;
        args.DoForAllType = DoForAllTypes.BlockingBulkUpdate;

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