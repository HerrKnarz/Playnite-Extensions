using LinkUtilities.Helper;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers.GamesCommon;
using System.Net;

namespace LinkUtilities.LinkActions;

/// <summary>
/// Class to handle the actions received from the UriHandler. At the moment only adding links to the
/// active URL in the web browser.
/// </summary>
internal class HandleUriActions : BaseAction
{
    private static HandleUriActions? _instance;

    private HandleUriActions()
    { }

    /// <summary>
    /// Name of the link to be added in the "AddLink" action
    /// </summary>
    public string? LinkName { get; set; }

    /// <summary>
    /// List of patterns to find the right link name for a given set of URL and link title
    /// </summary>
    public LinkNamePatterns? LinkNamePatterns { get; set; }

    /// <summary>
    /// URL of the link to be added in the "AddLink" action
    /// </summary>
    public string? LinkUrl { get; set; }

    public override string ProgressMessage => Loc.progress_adding_website_links();

    public override string ResultMessageId => LocId.dialog_added_links_message;

    public static HandleUriActions Instance() => _instance ??= new HandleUriActions();

    public override async Task<bool> ExecuteAsync(GameEx game, BaseActionArgs args)
    {
        if (await LinkHelper.AddLinkAsync(game.Game, LinkName, LinkUrl))
        {
            _gamesAffected.Add(game.Game);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Processes the arguments received from the UriHandler.
    /// </summary>
    /// <param name="args">Arguments to process</param>
    /// <returns>True if the arguments could be processed and fit a LinkUtilities action</returns>
    public async Task<bool> ProcessArgsAsync(PlayniteUriEventArgs args)
    {
        if (args is null
            || !args.Arguments.HasItems()
            || !args.Arguments[0].Equals("AddLink")
            || args.Arguments.Length != 3)
        {
            return false;
        }

        var tempLinkName = args.Arguments[1];
        LinkUrl = WebUtility.UrlDecode(args.Arguments[2]);

        if (LinkNamePatterns?.LinkMatch(ref tempLinkName, LinkUrl) ?? false)
        {
            LinkName = tempLinkName;
            return true;
        }

        if (LinkUtilitiesPlugin.PlayniteApi is null)
        {
            return false;
        }

        var selectResult = await LinkUtilitiesPlugin.PlayniteApi.Dialogs.SelectStringAsync(
            Loc.dialog_enter_link_name(),
            Loc.dialog_name_link_caption(),
            tempLinkName);

        if (!selectResult.Result)
        {
            return false;
        }

        LinkName = selectResult.SelectedString;

        return true;
    }

    public async Task UriHandlerAsync(PlayniteUriEventArgs args)
    {
        if (LinkUtilitiesPlugin.PlayniteApi is null || !await HandleUriActions.Instance().ProcessArgsAsync(args))
        {
            return;
        }

        var games = LinkUtilitiesPlugin.PlayniteApi.MainView.GetSelectedGames().Distinct().Select(g => new GameEx(g)).ToList();

        var actionArgs = new BaseActionArgs(LinkUtilitiesPlugin.PlayniteApi)
        {
            ShowDialogs = true
        };

        await DoForAllAsync(games, actionArgs);
    }
}