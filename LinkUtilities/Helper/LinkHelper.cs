using LinkUtilities.LinkActions;
using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.GamesCommon;
using PlayniteExtensionHelpers.MetadataCommon;
using System.Collections.ObjectModel;

namespace LinkUtilities.Helper;

/// <summary>
/// Helper class containing functions used in the link utilities extension
/// </summary>
public static class LinkHelper
{
    /// <summary>
    /// Removes specific links from one or more games.
    /// </summary>
    /// <param name="games">List of games</param>
    /// <param name="plugin">LinkUtilities plugin instance</param>
    //NEXT: Implement RemoveSpecificLinks
    /*
    public static void RemoveSpecificLinks(List<Game> games, LinkUtilitiesPlugin plugin)
    {
        var window = RemoveSpecificLinksViewModel.GetWindow(games, plugin);

        window?.ShowDialog();
    }
    */

    public static string GogId => "Crow.GOG";

    public static async Task<bool> AddExternalIdAsync(Game game, string? id, string value, string name)
    {
        if ((id.IsNullOrEmpty() && name.IsNullOrEmpty()) || value.IsNullOrEmpty())
        {
            return false;
        }

        if (LinkUtilitiesPlugin.PlayniteApi is null)
        {
            return false;
        }

        var type = await LibraryObjectHelper.GetLibraryObjectAsync(LinkUtilitiesPlugin.PlayniteApi.Library.ExternalIdentifierTypes, name, id);

        if (type is null)
        {
            return false;
        }

        id = type.Id;

        var externalId = new ExternalIdentifier(id, value);

        if (game.ExternalIdentifiers is null)
        {
            game.ExternalIdentifiers = [externalId];

            return true;
        }

        if (game.ExternalIdentifiers.Any(e => e.TypeId == id))
        {
            return false;
        }

        game.ExternalIdentifiers.Add(externalId);

        return true;
    }

    /// <summary>
    /// Adds a link to a game.
    /// </summary>
    /// <param name="game">Game the link will be added to</param>
    /// <param name="linkName">Name of the link</param>
    /// <param name="linkUrl">URL of the link</param>
    /// <param name="linkTypeId">
    /// Optional typeId of the link. If not specified, it will be generated from the name. Has to
    /// exist already if specified.
    /// </param>
    /// <param name="ignoreExisting">if true existing links of the same name will be ignored</param>
    /// <returns>
    /// True, if a link could be added. Returns false, if a link with that name was already present
    /// or couldn't be added.
    /// </returns>
    public static async Task<bool> AddLinkAsync(Game game, string? linkName, string? linkUrl, string? linkTypeId = null, bool ignoreExisting = true)
    {
        var mustUpdate = false;
        var addNewLink = false;

        if (linkName.IsNullOrEmpty() || linkUrl.IsNullOrEmpty())
        {
            return false;
        }

        if (linkTypeId.IsNullOrEmpty())
        {
            if (LinkUtilitiesPlugin.PlayniteApi is null)
            {
                return false;
            }

            var type = await LibraryObjectHelper.GetLibraryObjectAsync(LinkUtilitiesPlugin.PlayniteApi.Library.WebLinkTypes, linkName, linkTypeId);

            if (type is null)
            {
                return false;
            }

            linkTypeId = type.Id;
        }

        var link = new WebLink(linkTypeId, linkUrl);

        // If the game doesn't have any Links yet, we have to add the collection itself.
        if (game.Links is null)
        {
            game.Links = [link];
            mustUpdate = true;
        }
        // otherwise we'll check if a link with the specified name is already present. If not, we'll
        // add the link and return true.
        else
        {
            if (!LinkExists(game, link.TypeId))
            {
                addNewLink = true;
            }
            else if (!ignoreExisting)
            {
                // NEXT: Check if I need to check for existing ones again.
                // NEXT: Implement special dialog with option to choose link type or add a new one.
                StringSelectionDialogResult? selectResult = null;

                if (LinkUtilitiesPlugin.Settings.OnlyATest)
                {
                    selectResult = new StringSelectionDialogResult(true, $"{link.TypeId}Test");
                }
                else
                {
                    if (LinkUtilitiesPlugin.PlayniteApi is not null)
                    {
                        selectResult = await LinkUtilitiesPlugin.PlayniteApi.Dialogs.SelectStringAsync(
                            Loc.dialog_replace_link(linkName),
                            Loc.dialog_select_option(),
                            linkName ?? string.Empty);
                    }
                }

                if (selectResult is null || !selectResult.Result)
                {
                    return false;
                }

                if (linkName != selectResult.SelectedString)
                {
                    if (LinkUtilitiesPlugin.PlayniteApi is null)
                    {
                        return false;
                    }

                    var type = await LibraryObjectHelper.GetLibraryObjectAsync(LinkUtilitiesPlugin.PlayniteApi.Library.WebLinkTypes, selectResult.SelectedString);

                    if (type == null)
                    {
                        return false;
                    }

                    link.TypeId = type.Id;
                    addNewLink = true;
                }
                else
                {
                    UIDispatcher.Invoke(delegate
                    {
                        game.Links.First(x => x.TypeId == link.TypeId).Url = link.Url;
                    });

                    mustUpdate = true;
                }
            }

            if (addNewLink)
            {
                UIDispatcher.Invoke(delegate
                {
                    game.Links.Add(link);
                });

                mustUpdate = true;
            }
        }

        return mustUpdate;
    }

    /// <summary>
    /// Adds a list of links to a game.
    /// </summary>
    /// <param name="game">Game the links will be added to</param>
    /// <param name="links">the links to add</param>
    /// <returns>
    /// True, if at least one link could be added. Returns false, if the links already existed or
    /// couldn't be added.
    /// </returns>
    public static async Task<bool> AddLinksAsync(Game game, List<WebLink> links)
    {
        if (!links.HasItems())
        {
            return false;
        }

        var mustUpdate = false;

        // If the game doesn't have any Links yet, we have to add the collection itself.
        if (game.Links is null)
        {
            game.Links = new ObservableCollection<WebLink>(links);
            mustUpdate = true;
        }
        // otherwise we'll check if links with the specified types are already present, add the
        // missing ones and return true if at least one was added.
        else
        {
            UIDispatcher.Invoke(delegate
            {
                mustUpdate = game.Links.AddMissing(links.Where(l => !LinkExists(game, l.TypeId)));
            });
        }

        return mustUpdate;
    }

    public static ExternalIdentifier? GetExternalId(Game game, string id) => game?.ExternalIdentifiers?.FirstOrDefault(e => e.TypeId == id);

    /// <summary>
    /// Checks if the game already has a link of the given type
    /// </summary>
    /// <param name="game">Game for which the Links will be checked</param>
    /// <param name="typeId">Type of the link</param>
    /// <returns>True, if a link with that type exists</returns>
    public static bool LinkExists(Game game, string? typeId) => !typeId.IsNullOrEmpty() && (game.Links?.Any(x => x.TypeId == typeId) ?? false);

    public static string? LinkNames(Game game)
    {
        return LinkUtilitiesPlugin.PlayniteApi is null || game is null || !game.Links.HasItems()
            ? null
            : string.Join(',', game.Links.Select(l => LinkUtilitiesPlugin.PlayniteApi.Library.WebLinkTypes.First(t => t.Id.Equals(l.TypeId)).Name).Distinct().OrderBy(s => s));
    }

    public static async Task<bool> UpdateGameInLibraryAsync(Game gameToUpdate, BaseActionGame processedGame, bool cleanUpAfterAdding = true)
    {
        var needsUpdate = false;

        if (processedGame.Game.ExternalIdentifiers.HasItems())
        {
            if (gameToUpdate.ExternalIdentifiers is null)
            {
                gameToUpdate.ExternalIdentifiers = processedGame.Game.ExternalIdentifiers;

                needsUpdate = true;
            }
            else
            {
                needsUpdate = gameToUpdate.ExternalIdentifiers.AddMissing(processedGame.Game.ExternalIdentifiers.Where(s => !gameToUpdate.ExternalIdentifiers.Any(t => t.TypeId == s.TypeId)));
            }
        }

        if (processedGame.Game.Links.HasItems())
        {
            if (gameToUpdate.Links is null)
            {
                gameToUpdate.Links = processedGame.Game.Links;

                needsUpdate = true;
            }
            else
            {
                needsUpdate |= gameToUpdate.Links.AddMissing(processedGame.Game.Links.Where(l => !LinkHelper.LinkExists(gameToUpdate, l.TypeId)));
            }
        }

        if (!cleanUpAfterAdding || LinkUtilitiesPlugin.PlayniteApi is null)
        {
            return needsUpdate;
        }

        var doAfterChange = new CleanUpLinks();
        var args = doAfterChange.GetActionArgs(LinkUtilitiesPlugin.PlayniteApi, [], Loc.link_utilities_name());

        needsUpdate |= await doAfterChange.ExecuteAsync(new BaseActionGame(gameToUpdate), args);

        return needsUpdate;
    }
}