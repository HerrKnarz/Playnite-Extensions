using LinkUtilities.Interfaces;
using LinkUtilities.LinkActions;
using LinkUtilities.Settings;
using LinkUtilities.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinkUtilities.Helper
{
    /// <summary>
    ///     Helper class containing functions used in the link utilities extension
    /// </summary>
    internal static class LinkHelper
    {
        //        private static readonly string _agentString =
        //            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
        public static Guid GogId = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");

        /// <summary>
        ///     Removes specific links from one or more games.
        /// </summary>
        /// <param name="games">List of games</param>
        /// <param name="plugin">LinkUtilities plugin instance</param>
        public static void RemoveSpecificLinks(List<Game> games, LinkUtilities plugin)
        {
            var window = RemoveSpecificLinksViewModel.GetWindow(games, plugin);

            window?.ShowDialog();
        }

        /// <summary>
        ///     Adds a link to the specified URL to a game.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <param name="linkName">Name of the link</param>
        /// <param name="linkUrl">URL of the link</param>
        /// <param name="ignoreExisting">if true existing links of the same name will be ignored</param>
        /// <param name="cleanUpAfterAdding">if true, the links will be cleaned up afterward</param>
        /// <returns>
        ///     True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        internal static bool AddLink(Game game, string linkName, string linkUrl, bool ignoreExisting = true, bool cleanUpAfterAdding = true)
            => AddLink(game, new Link(linkName, linkUrl), ignoreExisting, cleanUpAfterAdding);

        /// <summary>
        ///     Adds a link to a game.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <param name="link">the link to add</param>
        /// <param name="ignoreExisting">if true existing links of the same name will be ignored</param>
        /// <param name="cleanUpAfterAdding">if true, the links will be cleaned up afterward</param>
        /// <returns>
        ///     True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        internal static bool AddLink(Game game, Link link, bool ignoreExisting = true, bool cleanUpAfterAdding = true)
        {
            var mustUpdate = false;
            var addNewLink = false;
            var renameLink = true;

            // If the game doesn't have any Links yet, we have to add the collection itself.
            if (game.Links is null)
            {
                game.Links = new ObservableCollection<Link> { link };
                mustUpdate = true;
            }
            // otherwise we'll check if a link with the specified name is already present. If not, we'll add the link and return true.
            else
            {
                if (!LinkExists(game, link.Name))
                {
                    addNewLink = true;
                }
                else if (!ignoreExisting)
                {
                    var message = string.Format(ResourceProvider.GetString("LOCLinkUtilitiesDialogReplaceLink"),
                        link.Name);

                    var selectResult = GlobalSettings.Instance().OnlyATest
                        ? new StringSelectionDialogResult(true, $"{link.Name} (Test)")
                        : API.Instance.Dialogs.SelectString(
                            message,
                            ResourceProvider.GetString("LOCLinkUtilitiesDialogSelectOption"),
                            link.Name);

                    if (selectResult.Result)
                    {
                        if (link.Name != selectResult.SelectedString)
                        {
                            link.Name = selectResult.SelectedString;
                            addNewLink = true;
                            renameLink = false;
                        }
                        else
                        {
                            if (GlobalSettings.Instance().OnlyATest)
                            {
                                game.Links.Single(x => x.Name == link.Name).Url = link.Url;
                            }
                            else
                            {
                                API.Instance.MainView.UIDispatcher.Invoke(delegate
                                {
                                    game.Links.Single(x => x.Name == link.Name).Url = link.Url;
                                });
                            }

                            mustUpdate = true;
                        }
                    }
                }

                if (addNewLink)
                {
                    if (GlobalSettings.Instance().OnlyATest)
                    {
                        game.Links.Add(link);
                    }
                    else
                    {
                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            game.Links.Add(link);
                        });
                    }

                    mustUpdate = true;
                }
            }

            // Updates the game in the database if we added a new link.
            if (mustUpdate)
            {
                DoAfterAdd(game, cleanUpAfterAdding, renameLink);
            }

            return mustUpdate;
        }

        /// <summary>
        ///     Adds a list of links to a game.
        /// </summary>
        /// <param name="game">Game the links will be added to</param>
        /// <param name="links">the links to add</param>
        /// <param name="cleanUpAfterAdding">if true, the links will be cleaned up afterward</param>
        /// <returns>
        ///     True, if at least one link could be added. Returns false, if the links already existed or couldn't be added.
        /// </returns>
        internal static bool AddLinks(Game game, List<Link> links, bool cleanUpAfterAdding = true)
        {
            if (!links?.Any() ?? true)
            {
                return false;
            }

            var mustUpdate = false;

            // If the game doesn't have any Links yet, we have to add the collection itself.
            if (game.Links is null)
            {
                game.Links = new ObservableCollection<Link>(links);
                mustUpdate = true;
            }
            // otherwise we'll check if a link with the specified name is already present. If not, we'll add the link and return true.
            else
            {
                if (GlobalSettings.Instance().OnlyATest)
                {
                    mustUpdate = game.Links.AddMissing(links);
                }
                else
                {
                    API.Instance.MainView.UIDispatcher.Invoke(delegate
                    {
                        mustUpdate = game.Links.AddMissing(links);
                    });
                }
            }

            // Updates the game in the database if we added a new link.
            if (mustUpdate)
            {
                DoAfterAdd(game, cleanUpAfterAdding);
            }

            return mustUpdate;
        }

        /// <summary>
        ///     Checks if the game already has a link with the given name
        /// </summary>
        /// <param name="game">Game for which the Links will be checked</param>
        /// <param name="linkName">Name of the link</param>
        /// <returns>True, if a link with that name exists</returns>
        internal static bool LinkExists(Game game, string linkName) =>
            game.Links?.Any(x => x.Name == linkName) ?? false;

        /// <summary>
        ///     Things to do after adding one or more links
        /// </summary>
        /// <param name="game">game to process</param>
        /// <param name="cleanUp">if true, the links will be cleaned up afterward</param>
        /// <param name="renameLinks">If True, newly added links will be renamed.</param>
        private static void DoAfterAdd(Game game, bool cleanUp = true, bool renameLinks = true)
        {
            if (!GlobalSettings.Instance().OnlyATest)
            {
                API.Instance.Database.Games.Update(game);
            }

            if (cleanUp)
            {
                DoAfterChange.Instance().Execute(game, renameLinks ? ActionModifierTypes.None : ActionModifierTypes.DontRename);
            }
        }
    }
}