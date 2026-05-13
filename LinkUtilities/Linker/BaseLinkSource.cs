using LinkUtilities.Helper;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.MetadataCommon;
using PlayniteExtensionHelpers.WebCommon;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace LinkUtilities.Linker;

/// <summary>
/// Defines the way a link can be added.
/// </summary>
public enum LinkAddTypes
{
    None,
    SingleSearchResult,
    UrlMatch
}

/// <summary>
/// Base class for a website link
/// </summary>
public abstract class BaseLinkSource(string id, LinkSourceArgs args)
{
    public ConcurrentQueue<string> TestResultQueue = new();

    /// <summary>
    /// Determines if a searched link should return a link if exactly one was found.
    /// </summary>
    public virtual bool AcceptSingleSearchResult => true;

    /// <summary>
    /// Specifies, if and how the link can be added without a search dialog
    /// </summary>
    public virtual LinkAddTypes AddType => LinkAddTypes.UrlMatch;

    /// <summary>
    /// Specifies urls that are allowed to return their status in the callback. It's used to add
    /// optional fallback urls when the desired url itself isn't found.
    /// </summary>
    public virtual HashSet<string> AllowedCallbackUrls { get; set; } = [];

    /// <summary>
    /// Base URL of the link before adding the specific path to the game itself. Only used if applicable.
    /// </summary>
    public virtual string BaseUrl => string.Empty;

    /// <summary>
    /// URL to use the search function of the website via browser. Can be different to the automatic
    /// search url, when APIs are involved for example.
    /// </summary>
    public virtual string BrowserSearchUrl => SearchUrl;

    /// <summary>
    /// Specifies, if the link is searchable via browser (e.g. the site has a search url than can be
    /// opened in a browser)
    /// </summary>
    public virtual bool CanBeBrowserSearched => !BrowserSearchUrl.IsNullOrWhiteSpace();

    /// <summary>
    /// Specifies, if the link is searchable (e.g. a search function via SearchUrl is implemented)
    /// </summary>
    public virtual bool CanBeSearched => !SearchUrl.IsNullOrWhiteSpace();

    /// <summary>
    /// String that has to be in the content of the page to determine, if the link is valid. Can be
    /// empty, if no check should be done.
    /// </summary>
    public virtual string CheckForContent { get; set; } = string.Empty;

    /// <summary>
    /// Delay in milliseconds between requests to the same website. Some sites block requests, when
    /// too many come in a short time.
    /// </summary>
    public virtual int Delay => 0;

    /// <summary>
    /// TypeId for an optional external ID that will be added to the game alongside the link if available.
    /// </summary>
    public virtual string ExternalIdType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the link source for background ops and dictionaries.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Specifies if the linker has been initialized.
    /// </summary>
    public bool Initialized { get; set; } = false;

    /// <summary>
    /// Name the link will have in the games link collection
    /// </summary>
    public virtual string LinkName { get; } = string.Empty;

    /// <summary>
    /// Arguments used to create an instance of the class.
    /// </summary>
    //NEXT: Check if LinkSourceArgs are really needed.
    public virtual LinkSourceArgs LinkSourceArgs { get; } = args;

    /// <summary>
    /// TypeID of the link.
    /// </summary>
    public virtual string LinkTypeId { get; set; } = string.Empty;

    /// <summary>
    /// The final URL for the link
    /// </summary>
    public virtual string LinkUrl { get; set; } = string.Empty;

    /// <summary>
    /// Specifies, if the link needs to be checked.
    /// </summary>
    public virtual bool NeedsToBeChecked { get; set; } = true;

    /// <summary>
    /// Web worker pipeline the class uses to search a link
    /// </summary>
    public virtual Pipeline? Pipeline { get; set; }

    /// <summary>
    /// Specifies the priority of the link source. Lower numbers mean higher priority and will be
    /// fetched earlier.
    /// </summary>
    public virtual int Priority => 1;

    /// <summary>
    /// Specifies, if the returned url must be the same as the searched one while checking the URL.
    /// Some sites always redirect and then return OK, even if the link isn't valid. In that case
    /// this should be set to true, if the url needs to be the same.
    /// </summary>
    public virtual bool ReturnsSameUrl { get; set; } = false;

    /// <summary>
    /// URL to use the search function of the website
    /// </summary>
    public virtual string SearchUrl => string.Empty;

    /// <summary>
    /// represents the settings for this specific link
    /// </summary>
    public LinkSourceSetting Settings { get; set; } = new();

    /// <summary>
    /// List of test cases to test the link source. Must be implemented in the derived class or the
    /// result will be an empty list.
    /// </summary>
    public virtual List<TestCase> TestCases => [];

    /// <summary>
    /// Determines if the link source is in test mode. In that case the test cases will be executed
    /// instead of the actual game.
    /// </summary>
    public virtual bool TestMode { get; set; } = false;

    /// <summary>
    /// Website title that is returned if the link wasn't valid. Can be used for websites that
    /// return status code 200 and still load a generic search or index page that isn't tied to the game.
    /// </summary>
    public virtual string WrongTitle { get; set; } = string.Empty;

    /// <summary>
    /// Adds a link and optionally an external id from a search result.
    /// </summary>
    /// <param name="game">Game the link will be added to</param>
    /// <param name="result">Search result with the link</param>
    /// <returns>True if the link and/or external id was added</returns>
    public virtual async Task<bool> AddLinkFromSearchAsync(Game game, LinkSearchResult result)
    {
        if (!result.Id.IsNullOrEmpty())
        {
            await LinkHelper.AddExternalIdAsync(game, ExternalIdType, result.Id, LinkName);
        }

        return await LinkHelper.AddLinkAsync(game, LinkName, result.Url, LinkTypeId, false, true);
    }

    /// <summary>
    /// Checks if the link is valid.
    /// </summary>
    /// <param name="link">The link to check</param>
    /// <returns>True, if the link is valid</returns>
    public virtual async Task<bool> CheckLinkAsync(string link)
        => Pipeline is not null
        && await Pipeline.IsUrlOkAsync(link, ReturnsSameUrl, WrongTitle, CheckForContent, AllowedCallbackUrls);

    /// <summary>
    /// Finds one or more links without user interaction
    /// </summary>
    /// <param name="game">Game the link will be found for</param>
    /// <returns>List of found links and True, if a link was found</returns>
    public virtual async Task<(List<WebLink> links, bool result)> FindLinksAsync(Game game)
    {
        LinkUrl = string.Empty;

        var links = new List<WebLink>();

        if (TestMode)
        {
            await RunTests(game);

            return (links, true);
        }

        if (LinkHelper.LinkExists(game, LinkTypeId))
        {
            return (links, false);
        }

        switch (AddType)
        {
            case LinkAddTypes.SingleSearchResult:
                await GetSingleSearchResult(game);
                break;

            case LinkAddTypes.UrlMatch:
                await GetUrlMatchAsync(game);
                break;

            default:
                return (links, false);
        }

        if (LinkUrl.IsNullOrEmpty())
        {
            return (links, false);
        }

        links.Add(new(LinkTypeId, LinkUrl));

        return (links, true);
    }

    /// <summary>
    /// Returns the search link for a game on the website to be sent to the browser.
    /// </summary>
    /// <param name="game">Game the link will be added to</param>
    /// <returns>Link to the search page with the name of the game.</returns>
    public virtual string GetBrowserSearchLink(Game game) => BrowserSearchUrl + (game.Name.UrlEncode() ?? string.Empty);

    /// <summary>
    /// Determines the game path part of the link.
    /// </summary>
    /// <param name="game">Game the link will be added to</param>
    /// <param name="gameName">
    /// string we want to search for, if it's something else than the game name
    /// </param>
    /// <returns>Path that can be added to the BaseUrl to get the full link</returns>
    public virtual async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
    {
        gameName ??= game.Name;

        return gameName.IsNullOrEmpty() ? null : gameName;
    }

    /// <summary>
    /// gets a link via search dialog.
    /// </summary>
    /// <param name="game">Game the link will be searched for and added to</param>
    /// <param name="skipExistingLinks">When true already existing links will be skipped.</param>
    /// <returns>True, if a link was added</returns>
    public virtual async Task<bool> GetSearchedLinkAsync(Game game, bool skipExistingLinks = false)
    {
        if (LinkUtilitiesPlugin.PlayniteApi is null)
        {
            return false;
        }

        if (skipExistingLinks && LinkHelper.LinkExists(game, LinkTypeId))
        {
            return false;
        }

        ChooseDialogItem? result;

        if (LinkUtilitiesPlugin.Settings.OnlyATest)
        {
            var searchResults = await GetSearchResultsAsync(new ChooseItemWithSearchAsyncArgs(game.Name, new CancellationToken()));

            result = searchResults?.FirstOrDefault();
        }
        else
        {
            result = await LinkUtilitiesPlugin.PlayniteApi.Dialogs.ChooseItemWithSearchAsync(
                game.Name,
                GetSearchResultsAsync,
                $"{Loc.dialog_search_game()} ({LinkName})");
        }

        return result != null && await AddLinkFromSearchAsync(game, (LinkSearchResult)result);
    }

    /// <summary>
    /// Searches the website and returns a list of found games via GenericItemOption. An extended
    /// list with URL is also written to the list SearchResults. Must be implemented in the derived
    /// class or the result will be an empty list.
    /// </summary>
    /// <param name="searchArgs">
    /// Search arguments including the term to be searched for and a cancellation token.
    /// </param>
    /// <returns>List with all found games. Is an empty list in the base class.</returns>
    public virtual async Task<IEnumerable<ChooseDialogItem>> GetSearchResultsAsync(ChooseItemWithSearchAsyncArgs searchArgs) => [];

    public virtual async Task GetSingleSearchResult(Game game, string? gameName = null)
    {
        LinkUrl = string.Empty;

        if (!CanBeSearched)
        {
            return;
        }

        gameName ??= game.Name;

        if (gameName.IsNullOrEmpty())
        {
            return;
        }

        var baseName = gameName.RemoveEditionSuffix();

        var linkUrl = await TryToFindPerfectMatchingUrl(game, gameName)
            ?? (baseName == gameName ? await TryToFindPerfectMatchingUrl(game, baseName) : default);

        LinkUrl = linkUrl ?? string.Empty;
    }

    public virtual async Task<string?> GetUrlMatchAsync(Game game)
    {
        var gameName = await GetGamePathAsync(game);

        if (gameName.IsNullOrEmpty())
        {
            return gameName;
        }

        if (!NeedsToBeChecked || await CheckLinkAsync($"{BaseUrl}{gameName}"))
        {
            LinkUrl = $"{BaseUrl}{gameName}";

            return gameName;
        }

        var baseName = game.Name.RemoveEditionSuffix();

        if (baseName == game.Name)
        {
            return baseName;
        }

        gameName = await GetGamePathAsync(game, baseName);

        if (!NeedsToBeChecked || await CheckLinkAsync($"{BaseUrl}{gameName}"))
        {
            LinkUrl = $"{BaseUrl}{gameName}";
        }

        return gameName;
    }

    public virtual async Task InitializeAsync()
    {
        if (Initialized)
        {
            return;
        }

        Settings.Id = Id;
        Settings.LinkName = LinkName;
        Settings.IsAddable = AddType != LinkAddTypes.None ? true : null;
        Settings.IsSearchable = CanBeSearched ? true : null;

        var linkSettings = LinkUtilitiesPlugin.Settings?.LinkSettings.FirstOrDefault(s => s.Id == Id);

        if (linkSettings is not null)
        {
            Settings.ShowInMenus = linkSettings.ShowInMenus;
            Settings.ApiKey = linkSettings.ApiKey;
            Settings.NeedsApiKey = linkSettings.NeedsApiKey;
        }

        if (LinkTypeId.IsNullOrEmpty())
        {
            LinkTypeId = LinkName.ToTypeId() ?? string.Empty;
        }

        if (LinkUtilitiesPlugin.PlayniteApi is null)
        {
            Initialized = false;
            return;
        }

        var linkType = await LibraryObjectHelper.GetLibraryObjectAsync(LinkUtilitiesPlugin.PlayniteApi.Library.WebLinkTypes, LinkName, LinkTypeId);

        Initialized = linkType is not null;
    }

    public virtual async Task RunTests(Game game)
    {
        foreach (var testCase in TestCases)
        {
            if (testCase.GameName.IsNullOrEmpty())
            {
                continue;
            }

            game.Name = testCase.GameName;

            switch (AddType)
            {
                case LinkAddTypes.SingleSearchResult:
                    await GetSingleSearchResult(game);
                    testCase.GamePath = LinkUrl;
                    break;

                case LinkAddTypes.UrlMatch:
                    testCase.GamePath = await GetUrlMatchAsync(game);
                    break;

                case LinkAddTypes.None:
                default:
                    return;
            }

            testCase.Url = LinkUrl;

            var gamePathOk = testCase.GamePath == testCase.GamePathExpected;
            var urlOk = testCase.Url == testCase.UrlExpected;

            // TODO: Refine testing with better logging and/or even a window with the results.
            TestResultQueue.Enqueue($"\n============================ Test case {testCase.CaseName} ============================ " +
                        $"\nGamePathExp = {testCase.GamePathExpected}" +
                        $"\nGamePathGot = {testCase.GamePath} => {gamePathOk}" +
                        $"\nUrlExp = {testCase.UrlExpected}" +
                        $"\nUrlGot = {testCase.Url} => {urlOk}" +
                        $"\n======================================================== ");
        }
    }

    /// <summary>
    /// Opens a browser with the browser search url.
    /// </summary>
    /// <param name="game">Game the link will be searched for</param>
    public virtual void StartBrowserSearch(List<Game> games)
    {
        if (!games.HasItems())
        {
            return;
        }

        //NEXT: Add dialog to ask if the user really wants to open all the links, when there are more than 10 games.
        foreach (var game in games)
        {
            var url = GetBrowserSearchLink(game);

            if (url.IsNullOrEmpty())
            {
                continue;
            }

            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
    }

    internal async Task<(bool Result, string? PageText)> LoadDocumentAsync(string url, string checkForContent = "", bool ignoreStatus = false, int delay = 0)
    {
        if (Pipeline is null)
        {
            return (false, null);
        }

        var loadUrlArgs = new LoadUrlArgs()
        {
            Url = url,
            DocumentType = DocumentType.Source,
            CheckForContent = checkForContent,
            AllowedCallbackUrls = AllowedCallbackUrls,
            DelayAfterNavigation = delay
        };

        var urlLoadResult = await Pipeline.LoadUrlAsync(loadUrlArgs);

        if ((!ignoreStatus && urlLoadResult.StatusCode != HttpStatusCode.OK) || urlLoadResult.ErrorDetails.Length > 0 || urlLoadResult.PageText.IsNullOrEmpty())
        {
            return (false, null);
        }

        return (true, urlLoadResult.PageText);
    }

    /// <summary>
    /// Searches for a game by name and looks for a matching search result.
    /// </summary>
    /// <param name="game">Game to add the link to</param>
    /// <param name="gameName">Name of the game. Can differ from the actual name of the game.</param>
    /// <returns>Url of the game. Returns null if no match was found.</returns>
    private async Task<string?> TryToFindPerfectMatchingUrl(Game game, string? gameName)
    {
        var searchResults = await GetSearchResultsAsync(new ChooseItemWithSearchAsyncArgs(gameName, new CancellationToken()));

        if (!searchResults.HasItems())
        {
            return null;
        }

        var searchName = gameName?.NormalizeSearchTerm();

        if (!searchResults.HasItems())
        {
            return null;
        }

        var foundGame = (LinkSearchResult?)searchResults.FirstOrDefault(r => r.Name.NormalizeSearchTerm() == searchName);

        if (foundGame is not null && !foundGame.Id.IsNullOrEmpty())
        {
            await LinkHelper.AddExternalIdAsync(game, ExternalIdType, foundGame.Id, LinkName);
        }

        return foundGame?.Url;
    }
}