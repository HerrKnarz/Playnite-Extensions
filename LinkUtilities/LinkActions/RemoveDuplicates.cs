using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers.GamesCommon;
using PlayniteExtensionHelpers.WebCommon;
using System.Collections.ObjectModel;

namespace LinkUtilities.LinkActions;

internal class RemoveDuplicates : BaseAction
{
    private static RemoveDuplicates? _instance;

    private RemoveDuplicates()
    { }

    public override string ProgressMessage => Loc.progress_removing_duplicates();

    public override string ResultMessageId => LocId.dialog_removed_duplicates_message;

    public static RemoveDuplicates Instance() => _instance ??= new RemoveDuplicates();

    public static void ShowReviewDuplicatesView(List<Game> games)
    {
        //NEXT: Implement duplicates view
        /*try
        {
            var viewModel = new ReviewDuplicatesViewModel(games);

            if (!viewModel.ReviewDuplicates?.Any() ?? true)
            {
                API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCLinkUtilitiesDialogNoDuplicatesFound"));
                return;
            }

            var window = WindowHelper.CreateSizeToContentWindow(ResourceProvider.GetString("LOCLinkUtilitiesReviewDuplicatesWindowName"));
            var view = new ReviewDuplicatesView { DataContext = viewModel };

            window.Content = view;

            window.ShowDialog();
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Error during initializing ReviewDuplicatesView", true);
        }*/
    }

    public override async Task<bool> ExecuteAsync(GameEx game, BaseActionArgs args)
    {
        if (RemoveDuplicateLinks(game.Game, LinkUtilitiesPlugin.Settings.RemoveDuplicatesType))
        {
            _gamesAffected.Add(game.Game);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes duplicate links from a game
    /// </summary>
    /// <param name="game">Game in which the duplicates will be removed.</param>
    /// <param name="duplicateType">
    /// Specifies, if the duplicates will be identified by name, URL or both.
    /// </param>
    /// <returns>
    /// True, if duplicates were removed. Returns false if there weren't duplicates to begin with.
    /// </returns>
    private static bool RemoveDuplicateLinks(Game game, DuplicateTypes duplicateType)
    {
        if (!game.Links.HasItems())
        {
            return false;
        }

        var linkCount = game.Links.Count;

        ObservableCollection<WebLink> newLinks;

        switch (duplicateType)
        {
            case DuplicateTypes.TypeAndUrl:
                newLinks = new ObservableCollection<WebLink>(game.Links
                    .GroupBy(x => new { x.TypeId, url = WebHelper.CleanUpUrl(x.Url) }).Select(x => x.First()));
                break;

            case DuplicateTypes.Type:
                newLinks = new ObservableCollection<WebLink>(
                    game.Links.GroupBy(x => x.TypeId).Select(x => x.First()));
                break;

            case DuplicateTypes.Url:
                newLinks = new ObservableCollection<WebLink>(game.Links.GroupBy(x => WebHelper.CleanUpUrl(x.Url))
                    .Select(x => x.First()));
                break;

            default:
                return false;
        }

        if (newLinks.Count >= linkCount)
        {
            return false;
        }

        game.Links = newLinks;

        return true;
    }
}