using HtmlAgilityPack;
using Playnite.SDK;
using Playnite.SDK.Models;
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
        /// <summary>
        /// Adds a link to the specified URL to a game.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <param name="linkName">Name of the link</param>
        /// <param name="linkUrl">URL of the link</param>
        /// <returns>
        /// True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        public static bool AddLink(Game game, string linkName, string linkUrl, LinkUtilitiesSettings settings, bool ignoreExisting = true)
        {
            Link link = new Link(linkName, linkUrl);
            bool mustUpdate = false;

            // If the game doesn't have any Links yet, we have to add the collection itself.
            if (game.Links is null)
            {
                game.Links = new ObservableCollection<Link> { link };
                mustUpdate = true;
            }
            // otherwise we'll check if a link with the specified name is already present. If not, we'll add the link and return true.
            else
            {
                bool addNewLink = false;

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

                    // We sort the Links automatically if the setting SortAfterChange is true.
                    if (settings.SortAfterChange)
                    {
                        game.Links = new ObservableCollection<Link>(game.Links.OrderBy(x => x.Name));
                    }

                    mustUpdate = true;
                }
            }

            // Updates the game in the database if we added a new link.
            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }

        /// <summary>
        /// Checks if the game already has a link with the given name
        /// </summary>
        /// <param name="game">Game for which the Links will be checked</param>
        /// <param name="linkName">Name of the link</param>
        /// <returns>True, if a link with that name exists</returns>
        public static bool LinkExists(Game game, string linkName)
        {
            if (game.Links is null)
            {
                return false;
            }
            else
            {
                return game.Links.Count(x => x.Name == linkName) > 0;
            }
        }

        /// <summary>
        /// Sorts the Links of a game alphabetically by the link name.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static bool SortLinks(Game game)
        {
            if (game.Links != null && game.Links.Count > 0)
            {
                game.Links = new ObservableCollection<Link>(game.Links.OrderBy(x => x.Name));

                API.Instance.Database.Games.Update(game);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// PreRequest event for the HtmlWeb class. Is used to desable redirects,
        /// </summary>
        /// <param name="request">The request to be executed</param>
        /// <returns>True, if the request can be executed.</returns>
        private static bool OnPreRequest(HttpWebRequest request)
        {
            request.AllowAutoRedirect = AllowRedirects;
            return true;
        }

        private static bool AllowRedirects = true;

        /// <summary>
        /// Checks if an URL is reachable and returns OK
        /// </summary>
        /// <param name="url">URL to check</param>
        /// <returns>True, if the URL is reachable</returns>        
        public static bool CheckUrl(string url, bool allowRedirects = true)
        {
            AllowRedirects = allowRedirects;

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
