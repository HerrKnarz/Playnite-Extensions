using LinkUtilities.Helper;
using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.BaseClasses;

internal abstract class LibraryLinker : Linker
{
    public abstract string Id { get; }

    /// <summary>
    /// Adds a link to the specific game page of the library.
    /// </summary>
    /// <param name="game">Game the link will be added to</param>
    /// <returns>
    /// List of found links and True, if links could be added. Returns an empty list and false, if a
    /// link to the library was already present or couldn't be added.
    /// </returns>
    public abstract Task<(List<WebLink> links, bool result)> FindLibraryLinkAsync(Game game, string? gameId);

    public override async Task<(List<WebLink> links, bool result)> FindLinksAsync(Game game)
    {
        string? gameId = null;

        if (game.LibraryId == Id)
        {
            gameId = game.LibraryGameId;
        }
        else if (!ExternalIdType.IsNullOrEmpty())
        {
            gameId = LinkHelper.GetExternalId(game, ExternalIdType)?.IdValue;
        }

        return gameId is not null ? await FindLibraryLinkAsync(game, gameId) : await base.FindLinksAsync(game);
    }
}