using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.MetadataCommon;
using System.Collections.ObjectModel;

namespace LinkUtilities.Helper;

/// <summary>
/// Helper class containing functions used in the link utilities extension
/// </summary>
internal static class LinkHelper
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

    public static ExternalIdentifier? GetExternalId(Game game, string id) => game?.ExternalIdentifiers?.FirstOrDefault(e => e.TypeId == id);

    public static string? LinkNames(Game game)
    {
        return LinkUtilitiesPlugin.PlayniteApi is null || game is null || !game.Links.HasItems()
            ? null
            : string.Join(',', game.Links.Select(l => LinkUtilitiesPlugin.PlayniteApi.Library.WebLinkTypes.First(t => t.Id.Equals(l.TypeId)).Name).Distinct().OrderBy(s => s));
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
    /// <param name="cleanUpAfterAdding">if true, the links will be cleaned up afterward</param>
    /// <returns>
    /// True, if a link could be added. Returns false, if a link with that name was already present
    /// or couldn't be added.
    /// </returns>
    internal static async Task<bool> AddLinkAsync(Game game, string? linkName, string? linkUrl, string? linkTypeId = null, bool ignoreExisting = true, bool cleanUpAfterAdding = true)
    {
        var mustUpdate = false;
        var addNewLink = false;
        var replaceUrl = true;

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
                    replaceUrl = false;
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

        // Updates the game in the database if we added a new link.
        if (mustUpdate)
        {
            //NEXT: See how I can combine them all, since the games won't be updated in the loop anymore,
            await DoAfterAddAsync(game, cleanUpAfterAdding, replaceUrl);
        }

        return mustUpdate;
    }

    /// <summary>
    /// Adds a list of links to a game.
    /// </summary>
    /// <param name="game">Game the links will be added to</param>
    /// <param name="links">the links to add</param>
    /// <param name="cleanUpAfterAdding">if true, the links will be cleaned up afterward</param>
    /// <returns>
    /// True, if at least one link could be added. Returns false, if the links already existed or
    /// couldn't be added.
    /// </returns>
    internal static async Task<bool> AddLinksAsync(Game game, List<WebLink> links, bool cleanUpAfterAdding = true)
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

        // Updates the game in the database if we added a new link.
        if (mustUpdate)
        {
            await DoAfterAddAsync(game, cleanUpAfterAdding);
        }

        return mustUpdate;
    }

    /// <summary>
    /// Checks if the game already has a link of the given type
    /// </summary>
    /// <param name="game">Game for which the Links will be checked</param>
    /// <param name="typeId">Type of the link</param>
    /// <returns>True, if a link with that type exists</returns>
    internal static bool LinkExists(Game game, string? typeId) => !typeId.IsNullOrEmpty() && (game.Links?.Any(x => x.TypeId == typeId) ?? false);

    /// <summary>
    /// Things to do after adding one or more links
    /// </summary>
    /// <param name="game">game to process</param>
    /// <param name="cleanUp">if true, the links will be cleaned up afterward</param>
    /// <param name="renameLinks">If True, newly added links will be renamed.</param>
    private static async Task DoAfterAddAsync(Game game, bool cleanUp = true, bool renameLinks = true)
    {
        if (cleanUp)
        {
            //NEXT: Implement DoAfterChange
            //DoAfterChange.Instance().Execute(game, renameLinks ? ActionModifierTypes.None : ActionModifierTypes.DontRename);
        }
    }
}