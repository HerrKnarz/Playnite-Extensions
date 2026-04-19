using LinkUtilities.Helper;
using LinkUtilities.Linker;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

/// <summary>
/// Adds a link to the game store page of the library (e.g. Steam or GOG) the game is part of.
/// </summary>
internal class AddLibraryLinks : BaseAction
{
    /// <summary>
    /// contains all game LibraryLinks that have a link to a store page that can be added.
    /// </summary>
    public readonly LibraryLinks LibraryLinks;

    private static AddLibraryLinks? _instance;

    private AddLibraryLinks()
    {
        LibraryLinks = [];
    }

    public override string ProgressMessage => Loc.progress_adding_library_links();
    public override string ResultMessageId => LocId.dialog_added_links_message;

    public static AddLibraryLinks Instance() => _instance ??= new AddLibraryLinks();

    public override async Task<bool> ExecuteAsync(GameEx game, BaseActionArgs args)
    {
        if (!LibraryLinks.TryGetValue(game.Game.LibraryId, out var lib))
        {
            return false;
        }

        var (links, result) = await lib.FindLinksAsync(game.Game);

        if (result && links.HasItems() && await LinkHelper.AddLinksAsync(game.Game, links))
        {
            _gamesAffected.Add(game.Game);
            return true;
        }

        return false;
    }

    public override async Task FollowUpAsync(BaseActionArgs args)
    {
        await base.FollowUpAsync(args);

        foreach (var linker in LibraryLinks)
        {
            linker.Value.Pipeline?.Dispose();
            linker.Value.Pipeline = null;
        }
    }

    public override async Task<bool> PrepareAsync(BaseActionArgs args)
    {
        foreach (var linker in LibraryLinks)
        {
            linker.Value.Pipeline = new Pipeline(0);
        }

        return true;
    }
}