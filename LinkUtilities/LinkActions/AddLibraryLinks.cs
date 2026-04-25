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

    public override string Id => "linkutilities.library.links";

    public override string Name => Loc.action_name_library_links();

    public static AddLibraryLinks Instance() => _instance ??= new AddLibraryLinks();

    public override async Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args)
    {
        if (!LibraryLinks.TryGetValue(game.Game.LibraryId, out var lib))
        {
            return false;
        }

        var (links, result) = await lib.FindLinksAsync(game.Game);

        return result && links.HasItems() && await LinkHelper.AddLinksAsync(game.Game, links);
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

    public override BaseActionArgs GetActionArgs(IPlayniteApi api, List<BaseActionGame> games, string pluginName)
    {
        var args = base.GetActionArgs(api, games, pluginName);

        args.ProgressMessage = Loc.progress_adding_library_links();
        args.ResultMessageId = LocId.dialog_added_links_message;

        return args;
    }

    public override async Task<bool> PrepareAsync(BaseActionArgs args)
    {
        foreach (var linker in LibraryLinks)
        {
            linker.Value.Pipeline = new Pipeline(0);
        }

        return true;
    }

    public override bool ProcessUpdateData(Game gameToUpdate, BaseActionGame processedGame)
                                    => LinkHelper.UpdateGameInLibrary(gameToUpdate, processedGame);
}