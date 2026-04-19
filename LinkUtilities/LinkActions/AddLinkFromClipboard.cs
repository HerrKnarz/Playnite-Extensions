using LinkUtilities.Helper;
using Playnite;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

internal class AddLinkFromClipboard : BaseAction
{
    private static AddLinkFromClipboard? _instance;

    private AddLinkFromClipboard()
    { }

    /// <summary>
    /// Name of the link to be added in the "AddLink" action
    /// </summary>
    public string? LinkName { get; set; }

    /// <summary>
    /// URL of the link to be added in the "AddLink" action
    /// </summary>
    public string? LinkUrl { get; set; }

    public override string ProgressMessage => Loc.progress_adding_website_links();

    public override string ResultMessageId => LocId.dialog_added_links_message;

    public static AddLinkFromClipboard Instance() => _instance ??= new AddLinkFromClipboard();

    public override async Task<bool> ExecuteAsync(GameEx game, BaseActionArgs args)
    {
        if (await LinkHelper.AddLinkAsync(game.Game, LinkName, LinkUrl, null, false))
        {
            _gamesAffected.Add(game.Game);

            return true;
        }

        return false;
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