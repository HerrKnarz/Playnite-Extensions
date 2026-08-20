using AngleSharp;
using AngleSharp.Dom;
using KNARZhelper;
using KNARZhelper.WebCommon;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using UVLMetadata.Models;
using UVLMetadata.Parser;

namespace UVLMetadata;

public enum AuthenticationStatus
{
    NotAuthenticated,
    Authenticated,
    Unknown
}

/// <summary>
/// Handles all website calls to UVL
/// </summary>
public class UVLConnect(UVLMetadata plugin)
{
    public IDocument searchedDocument = null;
    private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
    private readonly LinkWorker _linkWorker = new(1);
    private readonly string _loginUrl = $"{Resources.WebsiteUrl}/admin/login.php";
    private readonly string _profileUrl = $"{Resources.WebsiteUrl}/me/";
    private readonly string _searchUrl = $"{Resources.WebsiteUrl}/globalsearch/?t=";

    public PlatformHelper PlatformHelper { get; } = new(API.Instance.Database.Platforms);

    public UVLTags Tags => plugin.Tags;

    public AuthenticationStatus Authenticate()
    {
        using var onScreenWebView = API.Instance.WebViews.CreateView(800, 800);
        try
        {
            onScreenWebView.DeleteDomainCookiesRegex(@"uvlist\.net");
            onScreenWebView.Navigate(_loginUrl);
            var isLoggedIn = AuthenticationStatus.Unknown;

            var firstPageAfterLogin = false;

            onScreenWebView.LoadingChanged += async (sender, args) =>
            {
                var address = onScreenWebView.GetCurrentAddress();

                if (!args.IsLoading)
                {
                    if (!firstPageAfterLogin && address.StartsWith(_profileUrl, StringComparison.InvariantCultureIgnoreCase))
                    {
                        firstPageAfterLogin = true;

                        isLoggedIn = AuthenticationStatus.Authenticated;

                        onScreenWebView.Close();
                    }
                }
            };

            onScreenWebView.OpenDialog();

            return isLoggedIn;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to authenticate with UVL");
            return AuthenticationStatus.NotAuthenticated;
        }
        finally
        {
            onScreenWebView.Dispose();
        }
    }

    /// <summary>
    /// Tries to find a single game based on the given name.
    /// </summary>
    /// <param name="gameName">Name of the game to find</param>
    /// <returns>
    /// Found game as a json result. Returns null if no confident single result was found.
    /// </returns>
    public UVLItemOption FindGame(Game game)
    {
        var compareName = game.Name.NormalizeSearchTerm();
        var searchName = game.Name.RemoveEditionSuffix();

        // We search for the game name on UVL
        var searchResults = GetSearchResults(searchName);

        searchName = searchName.NormalizeSearchTerm();

        var result =
            searchResults?.FirstOrDefault(p => p.Name.NormalizeSearchTerm().Equals(compareName, StringComparison.InvariantCultureIgnoreCase)
               && game.Platforms.Any(gp => gp.Name == ((UVLItemOption)p).Platform))
            ?? searchResults?.FirstOrDefault(p => p.Name.NormalizeSearchTerm().Equals(searchName, StringComparison.InvariantCultureIgnoreCase)
                && game.Platforms.Any(gp => gp.Name == ((UVLItemOption)p).Platform))
            ?? searchResults?.FirstOrDefault(p => p.Name.NormalizeSearchTerm().Equals(compareName, StringComparison.InvariantCultureIgnoreCase))
            ?? searchResults?.FirstOrDefault(p => p.Name.NormalizeSearchTerm().Equals(searchName, StringComparison.InvariantCultureIgnoreCase));

        return result is UVLItemOption uvlResult ? uvlResult : null;
    }

    public List<UVLItemOption> GetDetailSearchResults(string url, int gameCount)
    {
        var results = new List<UVLItemOption>();

        Cursor.Current = Cursors.WaitCursor;
        try
        {
            var globalProgressOptions = new GlobalProgressOptions(
                                    $"{ResourceProvider.GetString("LOCUVLMetadataProgressLoadingGames")} ...",
                                    true
                                )
            {
                IsIndeterminate = false
            };

            API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
            {
                try
                {
                    activateGlobalProgress.ProgressMaxValue = (int)Math.Ceiling((double)gameCount / 50);

                    var pageToProcess = true;
                    var startCount = 0;

                    while (pageToProcess)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        activateGlobalProgress.Text =
                            $"{ResourceProvider.GetString("LOCUVLMetadataProgressLoadingGames")} {startCount}/{gameCount}";

                        var uriBuilder = new UriBuilder(url);
                        var paramValues = HttpUtility.ParseQueryString(uriBuilder.Query);

                        if (startCount > 0)
                        {
                            paramValues.Set("listed", startCount.ToString());
                        }

                        uriBuilder.Query = paramValues.ToString();

                        var document = LoadDocument(uriBuilder.Uri.ToString());

                        if (document is null)
                        {
                            Log.Debug($"Failed to load document for detail search results: {uriBuilder.Uri}");
                            break;
                        }

                        results.AddRange(ProcessSearchResults(document.QuerySelectorAll("#gamesfound table > tbody > tr"), true));

                        var links = document.QuerySelectorAll(".toolbar > a.btn > i.material-icons");

                        if (links is null || !links.Any(l => l.TextContent.Contains("last_page")))
                        {
                            pageToProcess = false;
                            break;
                        }

                        startCount += 50;
                        Thread.Sleep(200);

                        if (activateGlobalProgress.CurrentProgressValue < activateGlobalProgress.ProgressMaxValue)
                        {
                            activateGlobalProgress.CurrentProgressValue++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }

        return results;
    }

    public IDocument GetDocFromString(string html) => AsyncHelper.RunSync(async () => await _context.OpenAsync(req => req.Content(html)));

    public IDocument GetGameData(string url) => LoadDocument(url);

    /// <summary>
    /// Searches for a game on UVL and returns a list of results
    /// </summary>
    /// <param name="searchTerm">Term to search for</param>
    /// <returns>List of found results</returns>
    public List<GenericItemOption> GetSearchResults(string searchTerm)
    {
        try
        {
            if (searchTerm.StartsWith("https://www.uvlist.net/game-"))
            {
                searchedDocument = GetGameData(searchTerm);

                var name = searchedDocument.QuerySelector("header h1")?.TextContent;

                if (name.IsNullOrEmpty())
                {
                    searchedDocument = null;
                    return null;
                }

                var resultList = new List<GenericItemOption>
                {
                    new UVLItemOption() {
                        Name = name,
                        Url = searchTerm,
                        Description = ResourceProvider.GetString("LOCUVLMetadataSearchDialogUrlFound"),
                    }
                };

                return resultList;
            }
            else
            {
                var document = LoadDocument($"{_searchUrl}{searchTerm.UrlEncode()}");

                var cells = document.QuerySelectorAll("div.card");

                if (cells is null || !cells.Any())
                {
                    return null;
                }
                else
                {
                    var results = ProcessSearchResults(cells.FirstOrDefault(c => c.QuerySelector("h2.card-header")?.TextContent == "Games by title")?.QuerySelectorAll("tbody > tr"));

                    var resultsAsGenericOptions = new List<GenericItemOption>();

                    resultsAsGenericOptions.AddRange(results);

                    return resultsAsGenericOptions;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }

        return null;
    }

    public AuthenticationStatus IsUserLoggedIn() =>
        LoadDocument(_profileUrl)?.QuerySelector("#dropdownMenuButton1") is not null ? AuthenticationStatus.Authenticated : AuthenticationStatus.NotAuthenticated;

    public IDocument LoadDocument(string url)
    {
        var urlLoadResult = _linkWorker.LoadUrl(url, DocumentType.Source, true, "", null, true);

        if ((urlLoadResult.StatusCode != HttpStatusCode.OK) || urlLoadResult.ErrorDetails.Length > 0 || string.IsNullOrEmpty(urlLoadResult.PageText))
        {
            Log.Debug($"Failed to load document: {url} - {urlLoadResult.StatusCode}");

            return null;
        }

        var document = GetDocFromString(urlLoadResult.PageText);

        return document;
    }

    public void RefreshTags()
    {
        Cursor.Current = Cursors.WaitCursor;
        try
        {
            var globalProgressOptions = new GlobalProgressOptions(
                                    $"{ResourceProvider.GetString("LOCUVLMetadataName")} - {ResourceProvider.GetString("LOCUVLMetadataProgressRefreshingTags")}",
                                    true
                                )
            {
                IsIndeterminate = false
            };

            API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
            {
                try
                {
                    activateGlobalProgress.ProgressMaxValue = plugin.Settings.Settings.TagCategories.Count(c => !c.Value.Url.IsNullOrEmpty());

                    plugin.Tags.Clear();

                    var parser = new GroupParser();

                    foreach (var category in plugin.Settings.Settings.TagCategories.OrderBy(v => v.Value.Name))
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (category.Value.Url.IsNullOrEmpty())
                        {
                            continue;
                        }

                        activateGlobalProgress.Text =
                            $"{ResourceProvider.GetString("LOCUVLMetadataName")}{Environment.NewLine}{ResourceProvider.GetString("LOCUVLMetadataProgressRefreshingTags")}{Environment.NewLine}{category.Value.Name}";
                        var document = LoadDocument(category.Value.Url);

                        plugin.Tags.AddRange(parser.Parse(document, category.Key));

                        Thread.Sleep(200);

                        activateGlobalProgress.CurrentProgressValue++;
                    }

                    plugin.Tags.SetCategoryCaptions();

                    plugin.Tags.Save();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
    }

    private List<UVLItemOption> ProcessSearchResults(IHtmlCollection<IElement> results, bool isDetailSearch = false)
    {
        if (!results?.Any() ?? true)
        {
            return null;
        }

        PlatformHelper.RefreshPlatformList(API.Instance.Database.Platforms);

        var dateSelector = isDetailSearch ? "td:nth-child(3)" : "td:nth-child(1)";
        var platformSelector = isDetailSearch ? "td:nth-child(4)" : "td:nth-child(3)";
        var companySelector = isDetailSearch ? "td:nth-child(2)" : "td:nth-child(4)";

        var resultList = new List<UVLItemOption>();

        foreach (var row in results)
        {
            var platform = row.QuerySelector(platformSelector)?.TextContent;

            if (!platform.IsNullOrEmpty())
            {
                var foundPlatform = PlatformHelper.GetPlatforms(platform).FirstOrDefault();

                if (foundPlatform != null)
                {
                    if (foundPlatform is MetadataSpecProperty specProperty)
                    {
                        platform = API.Instance.Database.Platforms.FirstOrDefault(p => p.SpecificationId == specProperty.Id)?.Name ?? platform;
                    }
                    else if (foundPlatform is MetadataNameProperty nameProperty)
                    {
                        platform = nameProperty.Name;
                    }
                }
            }

            resultList.Add(new UVLItemOption
            {
                Name = WebUtility.HtmlDecode(row.QuerySelector("td:nth-child(1) a")?.TextContent),
                Url = Resources.WebsiteUrl + row.QuerySelector("td:nth-child(1) a")?.GetAttribute("href"),
                ReleaseDate = row.QuerySelector(dateSelector)?.TextContent,
                Platform = platform,
                Description = WebUtility.HtmlDecode($"{row.QuerySelector(dateSelector)?.TextContent} - {platform} - {row.QuerySelector(companySelector)?.TextContent}")
            });
        }

        return resultList;
    }
}
