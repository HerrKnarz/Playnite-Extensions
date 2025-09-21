using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Interfaces;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using LinkUtilities.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace LinkUtilities.Helper
{
    public enum UrlLoadMethod
    {
        /// <summary>
        /// Loads only the header of the URL. This is fast, but many websites return 403 without a real browser.
        /// </summary>
        Header,
        /// <summary>
        /// Loads the URL via the simple Load method of HtmlAgilityPack
        /// </summary>
        Load,
        /// <summary>
        /// Loads the URL using HtmlAgilityPack via a browser instance. This is slower, but can handle
        /// more complex sites. It causes some websites to time out though.
        /// </summary>
        LoadFromBrowser,
        /// <summary>
        /// Loads the URL using an offscreen browser instance. This is slower, but can handle
        /// more complex sites. We don't get a StatusCode though, so the validity must be checked
        /// in the document content individually.
        /// </summary>
        OffscreenView
    }

    /// <summary>
    ///     Helper class containing functions used in the link utilities extension
    /// </summary>
    internal static class LinkHelper
    {
        private static bool _allowRedirects = true;

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
        ///     tries to reach a URL and returns response infos like status code.
        /// </summary>
        /// <param name="url">URL to check</param>
        /// <param name="allowRedirects">If true, a redirect will count as ok.</param>
        /// <param name="checkForContent">Content to check for</param>
        /// <returns>Response infos</returns>
        internal static UrlLoadResult CheckUrl(string url, UrlLoadMethod method = UrlLoadMethod.Load, bool allowRedirects = true, string checkForContent = "") =>
            LoadHtmlDocument(url, method, allowRedirects, false, checkForContent);

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
        /// <param name="checkForContent">Content to check for</param>
        /// <returns>True, if the URL is reachable</returns>
        internal static bool IsUrlOk(string url, UrlLoadMethod method = UrlLoadMethod.Load, bool allowRedirects = true, bool sameUrl = false, string wrongTitle = "", string checkForContent = "")
        {
            var linkCheckResult = CheckUrl(url, method, allowRedirects, checkForContent);

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
        ///     Loads an HTML document from a URL using the specified method.
        /// </summary>
        /// <param name="url">URL to load</param>
        /// <param name="method">Loading method</param>
        /// <param name="allowRedirects">If true, redirects are allowed</param>
        /// <param name="needDocument">
        ///     If true, the loaded document will be returned in the result. Set to false if you only want to check for validity
        ///     and don't need the actual document</param>
        /// <param name="checkForContent">
        ///     Content to check for. Is used to determine if the returned document is valid. For LoadFromBrowser it also is used
        ///     to determine if the document is fully loaded</param>
        /// <returns>Loading result</returns>
        internal static UrlLoadResult LoadHtmlDocument(string url, UrlLoadMethod method = UrlLoadMethod.Load, bool allowRedirects = false, bool needDocument = true, string checkForContent = "")
        {
            var result = new UrlLoadResult();

            try
            {
                HtmlWeb htmlWeb = null;
                HtmlAgilityPack.HtmlDocument document = null;
                object exception = null;

                switch (method)
                {
                    case UrlLoadMethod.Header:
                        (result.ResponseUrl, result.StatusCode, exception) = CheckUrlSimple(url, allowRedirects);
                        break;
                    case UrlLoadMethod.Load:
                        (htmlWeb, document, exception) = LoadHtmlDocumentSimple(url, allowRedirects);
                        break;
                    case UrlLoadMethod.LoadFromBrowser:
                        (htmlWeb, document, exception) = LoadHtmlDocumentFromBrowser(url, allowRedirects, checkForContent);
                        break;
                    default:
                        {
                            var htmlSource = string.Empty;

                            (htmlSource, result.ResponseUrl, result.StatusCode, exception) = LoadHtmlDocumentFromOffscreenView(url, checkForContent);

                            if (result.StatusCode != HttpStatusCode.OK)
                            {
                                break;
                            }

                            try
                            {
                                document = new HtmlAgilityPack.HtmlDocument();
                                document.LoadHtml(htmlSource);
                            }
                            catch (Exception ex)
                            {
                                exception = ex;
                            }

                            break;
                        }
                }

                if (exception is WebException webEx)
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

                    return result;
                }
                else if (exception is Exception ex)
                {
                    result.ErrorDetails = ex.Message;
                    Log.Error(ex, result.ErrorDetails);

                    return result;
                }

                if (method == UrlLoadMethod.Header)
                {
                    return result;
                }

                if (document == null)
                {
                    result.ErrorDetails = $"Error loading HTML document from {url}";
                    return result;
                }

                if (method != UrlLoadMethod.OffscreenView)
                {
                    result.StatusCode = htmlWeb.StatusCode;
                    result.ResponseUrl = htmlWeb.ResponseUri.AbsoluteUri;
                }

                result.PageTitle = document?.DocumentNode?.SelectSingleNode("html/head/title")?.InnerText.Trim();

                if (needDocument)
                {
                    result.Document = document;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorDetails = ex.Message;
                Log.Error(ex, $"Error loading HTML document from {url}");

                return result;
            }
        }

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
        ///     Creates a new instance of HtmlWeb with some default settings.
        /// </summary>
        /// <param name="allowRedirects">If true, redirects are allowed</param>
        /// <returns>Configured HtmlWeb instance</returns>
        private static HtmlWeb GetHtmlWeb(bool allowRedirects)
        {
            var web = new HtmlWeb
            {
                UseCookies = true,
                BrowserTimeout = new TimeSpan(0, 0, 10),
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
            };

            if (allowRedirects)
            {
                _allowRedirects = allowRedirects;
                web.PreRequest = OnPreRequest;
            }

            return web;
        }

        private static (HtmlWeb, HtmlAgilityPack.HtmlDocument, object) LoadHtmlDocumentSimple(string url, bool allowRedirects = false)
        {
            HtmlWeb htmlWeb = null;
            HtmlAgilityPack.HtmlDocument document = null;
            object exception = null;

            try
            {
                htmlWeb = GetHtmlWeb(allowRedirects);
                document = htmlWeb.Load(url);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return (htmlWeb, document, exception);
        }

        private static (HtmlWeb, HtmlAgilityPack.HtmlDocument, object) LoadHtmlDocumentFromBrowser(string url, bool allowRedirects = false, string checkForContent = "")
        {
            object web = null;
            object doc = null;
            object exception = null;

            var thread = new Thread(
                      () =>
                      {
                          try
                          {
                              var threadWeb = GetHtmlWeb(allowRedirects);

                              web = threadWeb;

                              doc = checkForContent.Any()
                                  ? threadWeb.LoadFromBrowser(url, o =>
                                  {
                                      var webBrowser = (WebBrowser)o;

                                      return webBrowser.Document.Body.InnerHtml.Contains(checkForContent);
                                  })
                                  : threadWeb.LoadFromBrowser(url);
                          }
                          catch (Exception ex)
                          {
                              exception = ex;
                          }
                      });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            if (!thread.Join(new TimeSpan(0, 0, 15)))
            {
                thread.Abort();
            }

            return (web as HtmlWeb, doc as HtmlAgilityPack.HtmlDocument, exception);
        }

        private static (string, string, HttpStatusCode, object) LoadHtmlDocumentFromOffscreenView(string url, string checkForContent = "")
        {
            string responseUrl = null;
            string htmlSource = null;
            var statusCode = HttpStatusCode.OK;
            object exception = null;

            try
            {
                var webViewSettings = new WebViewSettings
                {
                    JavaScriptEnabled = true,
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
                };

                var webView = API.Instance.WebViews.CreateOffscreenView(webViewSettings);

                webView.NavigateAndWait(url);

                responseUrl = webView.GetCurrentAddress();

                htmlSource = webView.GetPageSource();

                webView.Close();

                statusCode = !checkForContent.Any() || htmlSource.Contains(checkForContent) ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return (htmlSource, responseUrl, statusCode, exception);
        }

        private static (string, HttpStatusCode, object) CheckUrlSimple(string url, bool allowRedirects = false)
        {
            string responseUrl = null;
            var statusCode = HttpStatusCode.OK;
            object exception = null;

            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                request.AllowAutoRedirect = allowRedirects;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
                request.Timeout = 10000;
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    statusCode = response.StatusCode;
                    responseUrl = response.ResponseUri.AbsoluteUri;
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return (responseUrl, statusCode, exception);
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