using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Interfaces;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;

namespace LinkUtilities.Helper
{
    /// <summary>
    ///     Helper class containing functions used in the link utilities extension
    /// </summary>
    internal static class LinkHelper
    {
        private static bool _allowRedirects = true;

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
        ///     tries to reach a URL and returns response infos like status code.
        /// </summary>
        /// <param name="url">URL to check</param>
        /// <param name="allowRedirects">If true, a redirect will count as ok.</param>
        /// <returns>Response infos</returns>
        internal static LinkCheckResult CheckUrl(string url, bool allowRedirects = true)
        {
            _allowRedirects = allowRedirects;

            var result = new LinkCheckResult();

            try
            {
                var web = new HtmlWeb
                {
                    UseCookies = true,
                    PreRequest = OnPreRequest
                };

                var document = web.Load(url);

                result.StatusCode = web.StatusCode;
                result.ResponseUrl = web.ResponseUri.AbsoluteUri;
                result.PageTitle = document?.DocumentNode?.SelectSingleNode("html/head/title")?.InnerText.Trim();
            }
            catch (Exception ex)
            {
                if (ex is WebException webEx)
                {
                    if (webEx.Response != null)
                    {
                        var response = webEx.Response;
                        var dataStream = response.GetResponseStream();

                        if (dataStream != null)
                        {
                            var reader = new StreamReader(dataStream);
                            result.ErrorDetails = reader.ReadToEnd();

                            Log.Error(webEx, result.ErrorDetails);
                        }
                    }
                }
                else
                {
                    result.ErrorDetails = ex.Message;
                    Log.Error(ex, result.ErrorDetails);
                }
            }

            return result;
        }

        /// <summary>
        ///     Removes the scheme of a URL and adds a missing trailing slash. Is used to compare URLs with different schemes
        /// </summary>
        /// <param name="url">URL to clean up</param>
        /// <returns>cleaned up URL</returns>
        internal static string CleanUpUrl(string url)
        {
            try
            {
                var uri = new Uri(url);

                var urlWithoutScheme = uri.Host + uri.PathAndQuery + uri.Fragment;

                return !urlWithoutScheme.EndsWith("/") ? urlWithoutScheme + "/" : urlWithoutScheme;
            }
            catch (Exception)
            {
                return url;
            }
        }

        /// <summary>
        ///     Checks if a URL is reachable and returns OK
        /// </summary>
        /// <param name="url">URL to check</param>
        /// <param name="allowRedirects">If true, a redirect will count as ok.</param>
        /// <param name="sameUrl">When true the method only returns true, if the response url didn't change.</param>
        /// <param name="wrongTitle">Returns false, if the website has this title. Is used to detect certain redirects.</param>
        /// <returns>True, if the URL is reachable</returns>
        internal static bool IsUrlOk(string url, bool allowRedirects = true, bool sameUrl = false, string wrongTitle = "")
        {
            var linkCheckResult = CheckUrl(url, allowRedirects);

            return !linkCheckResult.ErrorDetails.Any() && (sameUrl
                       ? linkCheckResult.StatusCode == HttpStatusCode.OK && linkCheckResult.ResponseUrl == url
                       : linkCheckResult.StatusCode == HttpStatusCode.OK) &&
                   (wrongTitle == string.Empty || linkCheckResult.PageTitle != wrongTitle);
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

        /// <summary>
        ///     PreRequest event for the HtmlWeb class. Is used to disable redirects,
        /// </summary>
        /// <param name="request">The request to be executed</param>
        /// <returns>True, if the request can be executed.</returns>
        private static bool OnPreRequest(HttpWebRequest request)
        {
            request.AllowAutoRedirect = _allowRedirects;
            return true;
        }
    }
}