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
        /// <returns>
        /// True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        internal static bool AddLink(Game game, string linkName, string linkUrl, LinkUtilities plugin, bool ignoreExisting = true)
        {
            Link link = new Link(linkName, linkUrl);
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
                if (game.Links.Count(x => x.Name == linkName) == 0)
                {
                    addNewLink = true;
                }
                else if (!ignoreExisting)
                {
                    string message = string.Format(ResourceProvider.GetString("LOCLinkUtilitiesDialogReplaceLink"), linkName);

                    StringSelectionDialogResult selectResult = API.Instance.Dialogs.SelectString(
                            message,
                            ResourceProvider.GetString("LOCLinkUtilitiesDialogSelectOption"),
                            linkName);

                    if (selectResult.Result)
                    {
                        if (linkName != selectResult.SelectedString)
                        {
                            link.Name = selectResult.SelectedString;
                            addNewLink = true;

                        }
                        else
                        {
                            API.Instance.MainView.UIDispatcher.Invoke(delegate
                            {
                                game.Links.Single(x => x.Name == linkName).Url = linkUrl;
                            });

                            mustUpdate = true;
                        }
                    }
                }

                if (addNewLink)
                {
                    API.Instance.MainView.UIDispatcher.Invoke(delegate
                    {
                        game.Links.Add(link);
                    });

                    mustUpdate = true;
                }
            }

            // Updates the game in the database if we added a new link.
            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);

                // We sort the Links automatically if the setting SortAfterChange is true.
                if (addNewLink && LinkActions.SortLinks.GetInstance(plugin).SortAfterChange)
                {
                    LinkActions.SortLinks.GetInstance(plugin).Execute(game);
                }
                // We add/remove tags for missing links automatically if the setting TagMissingLinksAfterChange is true.
                if (addNewLink && TagMissingLinks.GetInstance(plugin).TagMissingLinksAfterChange)
                {
                    TagMissingLinks.GetInstance(plugin).Execute(game);
                }
            }

            return mustUpdate;
        }

        /// <summary>
        /// Checks if the game already has a link with the given name
        /// </summary>
        /// <param name="game">Game for which the Links will be checked</param>
        /// <param name="linkName">Name of the link</param>
        /// <returns>True, if a link with that name exists</returns>
        internal static bool LinkExists(Game game, string linkName) => !(game.Links is null) && game.Links.Count(x => x.Name == linkName) > 0;

        /// <summary>
        /// Sorts the Links of a game alphabetically by the link name.
        /// </summary>
        /// <param name="game">Game in which the links will be sorted.</param>
        /// <returns>True, if the links could be sorted</returns>
        internal static bool SortLinks(Game game)
        {
            if (game.Links != null && game.Links.Count > 0)
            {
                game.Links = new ObservableCollection<Link>(game.Links.OrderBy(x => x.Name));

                API.Instance.Database.Games.Update(game);

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
            if (game.Links != null && game.Links.Count > 0)
            {
                game.Links = new ObservableCollection<Link>(game.Links.OrderBy(x => GetSortPosition(x.Name, sortOrder)).ThenBy(x => x.Name));

                API.Instance.Database.Games.Update(game);

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
            if (game.Links != null && game.Links.Count > 0)
            {
                int linkCount = game.Links.Count;

                ObservableCollection<Link> newLinks;

                switch (duplicateType)
                {
                    case DuplicateTypes.NameAndUrl:
                        newLinks = new ObservableCollection<Link>(game.Links.GroupBy(x => new { x.Name, url = CleanUpUrl(x.Url) }).Select(x => x.First()));
                        break;
                    case DuplicateTypes.Name:
                        newLinks = new ObservableCollection<Link>(game.Links.GroupBy(x => x.Name).Select(x => x.First()));
                        break;
                    case DuplicateTypes.Url:
                        newLinks = new ObservableCollection<Link>(game.Links.GroupBy(x => CleanUpUrl(x.Url)).Select(x => x.First()));
                        break;
                    default:
                        return false;
                }

                if (newLinks.Count < linkCount)
                {
                    game.Links = newLinks;

                    API.Instance.Database.Games.Update(game);

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

                return !url.EndsWith("/") ? (urlWithoutScheme += "/") : urlWithoutScheme;
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
        private static int? GetSortPosition(string linkName, Dictionary<string, int> sortOrder) => sortOrder.TryGetValue(linkName, out int position) ? position : int.MaxValue;

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

                HtmlDocument doc = web.Load(url);

                return web.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    WebResponse response = ex.Response;
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string details = reader.ReadToEnd();

                    Log.Error(ex, details);
                }

                return false;
            }
        }
    }
}
