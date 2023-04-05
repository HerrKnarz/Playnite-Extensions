using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.LinkActions;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;

namespace LinkUtilities
{
    /// <summary>
    /// Helper class containing functions used in the link utilities extension
    /// </summary>
    internal static class LinkHelper
    {
        private static bool _allowRedirects = true;

        /// <summary>
        /// Adds a link to the specified URL to a game.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <param name="linkName">Name of the link</param>
        /// <param name="linkUrl">URL of the link</param>
        /// <param name="ignoreExisting">if true existing links of the same name will be ignored</param>
        /// <returns>
        /// True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        internal static bool AddLink(Game game, string linkName, string linkUrl, bool ignoreExisting = true)
            => AddLink(game, new Link(linkName, linkUrl), ignoreExisting);

        /// <summary>
        /// Adds a link to a game.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <param name="link">the link to add</param>
        /// <param name="ignoreExisting">if true existing links of the same name will be ignored</param>
        /// <returns>
        /// True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        internal static bool AddLink(Game game, Link link, bool ignoreExisting = true)
        {
            bool mustUpdate = false;
            bool addNewLink = false;

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
                    string message = string.Format(ResourceProvider.GetString("LOCLinkUtilitiesDialogReplaceLink"),
                        link.Name);

                    StringSelectionDialogResult selectResult = GlobalSettings.Instance().OnlyATest
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
                DoAfterAdd(game);
            }

            return mustUpdate;
        }

        /// <summary>
        /// Adds a list of links to a game.
        /// </summary>
        /// <param name="game">Game the links will be added to</param>
        /// <param name="links">the links to add</param>
        /// <returns>
        /// True, if at least one link could be added. Returns false, if the links already existed or couldn't be added.
        /// </returns>
        internal static bool AddLinks(Game game, List<Link> links)
        {
            bool mustUpdate;

            // If the game doesn't have any Links yet, we have to add the collection itself.
            if (game.Links is null)
            {
                game.Links = new ObservableCollection<Link>(links);
                mustUpdate = true;
            }
            // otherwise we'll check if a link with the specified name is already present. If not, we'll add the link and return true.
            else
            {
                mustUpdate = game.Links.AddMissing(links);

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
                DoAfterAdd(game);
            }

            return mustUpdate;
        }

        private static void DoAfterAdd(Game game)
        {
            if (!GlobalSettings.Instance().OnlyATest)
            {
                API.Instance.Database.Games.Update(game);
            }

            // We sort the Links automatically if the setting SortAfterChange is true.
            if (LinkActions.SortLinks.Instance().SortAfterChange)
            {
                LinkActions.SortLinks.Instance().Execute(game);
            }

            // We add/remove tags for missing links automatically if the setting TagMissingLinksAfterChange is true.
            if (TagMissingLinks.Instance().TagMissingLinksAfterChange)
            {
                TagMissingLinks.Instance().Execute(game);
            }
        }

        /// <summary>
        /// Checks if the game already has a link with the given name
        /// </summary>
        /// <param name="game">Game for which the Links will be checked</param>
        /// <param name="linkName">Name of the link</param>
        /// <returns>True, if a link with that name exists</returns>
        internal static bool LinkExists(Game game, string linkName) =>
            game.Links?.Any(x => x.Name == linkName) ?? false;

        /// <summary>
        /// Sorts the Links of a game alphabetically by the link name.
        /// </summary>
        /// <param name="game">Game in which the links will be sorted.</param>
        /// <returns>True, if the links could be sorted</returns>
        internal static bool SortLinks(Game game)
        {
            if (game.Links?.Any() ?? false)
            {
                game.Links = new ObservableCollection<Link>(game.Links.OrderBy(x => x.Name));

                if (!GlobalSettings.Instance().OnlyATest)
                {
                    API.Instance.Database.Games.Update(game);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Sorts the Links of a game according to a defined sort order.
        /// </summary>
        /// <param name="game">Game in which the links will be sorted.</param>
        /// <param name="sortOrder">Dictionary that contains the sort order.</param>
        /// <returns>True, if the links could be sorted</returns>
        internal static bool SortLinks(Game game, Dictionary<string, int> sortOrder)
        {
            if (game.Links?.Any() ?? false)
            {
                game.Links = new ObservableCollection<Link>(game.Links.OrderBy(x => GetSortPosition(x.Name, sortOrder))
                    .ThenBy(x => x.Name));

                if (!GlobalSettings.Instance().OnlyATest)
                {
                    API.Instance.Database.Games.Update(game);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes duplicate links from a game
        /// </summary>
        /// <param name="game">Game in which the duplicates will be removed.</param>
        /// <param name="duplicateType">Specifies, if the duplicates will be identified by name, URL or both..</param>
        /// <returns>True, if duplicates were removed. Returns false if there weren't duplicates to begin with.</returns>
        internal static bool RemoveDuplicateLinks(Game game, DuplicateTypes duplicateType)
        {
            if (game.Links?.Any() ?? false)
            {
                int linkCount = game.Links.Count;

                ObservableCollection<Link> newLinks;

                switch (duplicateType)
                {
                    case DuplicateTypes.NameAndUrl:
                        newLinks = new ObservableCollection<Link>(game.Links
                            .GroupBy(x => new { x.Name, url = CleanUpUrl(x.Url) }).Select(x => x.First()));
                        break;
                    case DuplicateTypes.Name:
                        newLinks = new ObservableCollection<Link>(
                            game.Links.GroupBy(x => x.Name).Select(x => x.First()));
                        break;
                    case DuplicateTypes.Url:
                        newLinks = new ObservableCollection<Link>(game.Links.GroupBy(x => CleanUpUrl(x.Url))
                            .Select(x => x.First()));
                        break;
                    default:
                        return false;
                }

                if (newLinks.Count < linkCount)
                {
                    game.Links = newLinks;

                    if (!GlobalSettings.Instance().OnlyATest)
                    {
                        API.Instance.Database.Games.Update(game);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the scheme of an URL and adds a missing trailing slash. Is used to compare URLs with different schemes
        /// </summary>
        /// <param name="url">URL to clean up</param>
        /// <returns>cleaned up URL</returns>
        private static string CleanUpUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);

                string urlWithoutScheme = uri.Host + uri.PathAndQuery + uri.Fragment;

                return !url.EndsWith("/") ? urlWithoutScheme + "/" : urlWithoutScheme;
            }
            catch (Exception)
            {
                return url;
            }
        }

        /// <summary>
        /// Gets the sort position of a link name in the dictionary. If nothing is found, max int is returned, so the link will
        /// be last.
        /// </summary>
        /// <param name="linkName">Name of the link to be sorted</param>
        /// <param name="sortOrder">Dictionary that contains the sort order.</param>
        /// <returns>Position in the sort order. The max int is returned, if the link name is not in the dictionary. That way
        /// those links will always appear after the defined order.</returns>
        private static int? GetSortPosition(string linkName, Dictionary<string, int> sortOrder) =>
            sortOrder.TryGetValue(linkName, out int position) ? position : int.MaxValue;

        /// <summary>
        /// PreRequest event for the HtmlWeb class. Is used to disable redirects,
        /// </summary>
        /// <param name="request">The request to be executed</param>
        /// <returns>True, if the request can be executed.</returns>
        private static bool OnPreRequest(HttpWebRequest request)
        {
            request.AllowAutoRedirect = _allowRedirects;
            return true;
        }

        /// <summary>
        /// Checks if an URL is reachable and returns OK
        /// </summary>
        /// <param name="url">URL to check</param>
        /// <param name="allowRedirects">If true, a redirect will count as ok.</param>
        /// <returns>True, if the URL is reachable</returns>
        internal static bool CheckUrl(string url, bool allowRedirects = true)
        {
            _allowRedirects = allowRedirects;

            try
            {
                HtmlWeb web = new HtmlWeb
                {
                    UseCookies = true,
                    PreRequest = OnPreRequest
                };

                web.Load(url);

                return web.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                {
                    return false;
                }

                WebResponse response = ex.Response;
                Stream dataStream = response.GetResponseStream();

                if (dataStream == null)
                {
                    return false;
                }

                StreamReader reader = new StreamReader(dataStream);
                string details = reader.ReadToEnd();

                Log.Error(ex, details);

                return false;
            }
        }
    }
}