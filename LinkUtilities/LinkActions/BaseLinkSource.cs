using LinkUtilities.Helper;
using LinkUtilities.Linker;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.GamesCommon;
using PlayniteExtensionHelpers.MetadataCommon;
using PlayniteExtensionHelpers.WebCommon;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace LinkUtilities.LinkActions;

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
public abstract class BaseLinkSource(string id, LinkSourceArgs args) : BaseAction
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
    /// Specifies, if a redirect is allowed while checking the URL. Some sites redirect to the
    /// homepage if the link isn't valid. In that case this should be set to false.
    /// </summary>
    public virtual bool AllowRedirects { get; set; } = true;

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
    public override string Id { get; } = id;

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
    /// Name of the link for display purposes
    /// </summary>
    public override string Name => LinkName;

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
    /// <param name="cleanUpAfterAdding">Determines if the clean up routines will be executed afterwards</param>
    /// <returns>True if the link and/or external id was added</returns>
    public virtual async Task<bool> AddLinkFromSearchAsync(Game game, LinkSearchResult result, bool cleanUpAfterAdding = true)
    {
        if (!result.Id.IsNullOrEmpty())
        {
            await LinkHelper.AddExternalIdAsync(game, ExternalIdType, result.Id, LinkName);
        }

        return await LinkHelper.AddLinkAsync(game, LinkName, result.Url, LinkTypeId, false, cleanUpAfterAdding);
    }

    /// <summary>
    /// Adds a link to the specific game page of the specified website.
    /// </summary>
    /// <param name="game">Game the link will be added to</param>
    /// <param name="isBulkAction">True if the method is called in a bulk action</param>
    /// <returns>
    /// True, if a link could be added. Returns false, if a link with that name was already present
    /// or couldn't be added.
    /// </returns>
    public virtual async Task<bool> AddLinksAsync(Game game, bool isBulkAction = true)
    {
        if (isBulkAction && (Delay > 0))
        {
            await Task.Delay(Delay);
        }

        var result = await FindLinksAsync(game);

        return result.result && await LinkHelper.AddLinksAsync(game, result.links);
    }

    /// <summary>
    /// Adds a link via search dialog.
    /// </summary>
    /// <param name="game">Game the link will be searched for and added to</param>
    /// <param name="skipExistingLinks">When true already existing links will be skipped.</param>
    /// <param name="cleanUpAfterAdding">if true, the links will be cleaned up afterward</param>
    /// <returns>True, if a link was added</returns>
    public virtual async Task<bool> AddSearchedLinkAsync(Game game, bool skipExistingLinks = false, bool cleanUpAfterAdding = true)
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

        return result != null && await AddLinkFromSearchAsync(game, (LinkSearchResult)result, cleanUpAfterAdding);
    }

    /// <summary>
    /// Checks if the link is valid.
    /// </summary>
    /// <param name="link">The link to check</param>
    /// <returns>True, if the link is valid</returns>
    public virtual async Task<bool> CheckLinkAsync(string link)
        => Pipeline is not null
        && await Pipeline.IsUrlOkAsync(link, ReturnsSameUrl, WrongTitle, LinkUtilitiesPlugin.Settings.DebugMode, CheckForContent, AllowedCallbackUrls);

    public override async Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args)
    {
        return args is WebsiteLinksArgs addArgs && addArgs.ActionType switch
        {
            LinkActionType.Add
                => await AddLinksAsync(game.Game, addArgs.IsBulkAction),
            LinkActionType.Search
                => await AddSearchedLinkAsync(game.Game, addArgs.OnlyMissingLinks),
            LinkActionType.BrowserSearch
                => StartBrowserSearch(game.Game),
            _ => throw new ArgumentOutOfRangeException(nameof(args), addArgs.ActionType, null),
        };
    }

    /// <summary>
    /// Finds one or more links without user interaction
    /// </summary>
    /// <param name="game">Game the link will be found for</param>
    /// <returns>List of found links and True, if a link was found</returns>
    public virtual async Task<(List<WebLink> links, bool result)> FindLinksAsync(Game game)
    {
        async Task<string?> GetLinkUrl()
        {
            switch (AddType)
            {
                case LinkAddTypes.SingleSearchResult:
                    LinkUrl = await GetGamePathAsync(game) ?? string.Empty;
                    return LinkUrl;

                case LinkAddTypes.UrlMatch:
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
                    else
                    {
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
                    }

                    return gameName;

                case LinkAddTypes.None:
                    return null;

                default:
                    return null;
            }
        }

        LinkUrl = string.Empty;

        var links = new List<WebLink>();

        if (TestMode)
        {
            foreach (var testCase in TestCases)
            {
                if (testCase.GameName.IsNullOrEmpty())
                { continue; }

                game.Name = testCase.GameName;

                testCase.GamePath = await GetLinkUrl();

                testCase.Url = LinkUrl;

                TestResultQueue.Enqueue($"\n============================ Test case {testCase.CaseName} ============================ " +
                            $"\nGamePathExp = {testCase.GamePathExpected}" +
                            $"\nGamePathGot = {testCase.GamePath}" +
                            $"\nUrlExp = {testCase.UrlExpected}" +
                            $"\nUrlGot = {testCase.Url}" +
                            $"\n======================================================== ");
            }

            return (links, true);
        }

        if (LinkHelper.LinkExists(game, LinkTypeId))
        {
            return (links, false);
        }

        await GetLinkUrl();

        if (LinkUrl.IsNullOrEmpty())
        {
            return (links, false);
        }

        links.Add(new(LinkTypeId, LinkUrl));

        return (links, true);
    }

    public override async Task FollowUpAsync(BaseActionArgs args)
    {
        await base.FollowUpAsync(args);

        Pipeline?.Dispose();
        Pipeline = null;
    }

    /// <summary>
    /// Creates the action arguments suiting this specific class.
    /// </summary>
    /// <param name="api">Instance of the Playnite api</param>
    /// <param name="games">List of games to process</param>
    /// <param name="pluginName">Name of the plugin</param>
    /// <returns></returns>
    public override WebsiteLinksArgs GetActionArgs(IPlayniteApi api, List<BaseActionGame> games, string pluginName)
    {
        return new WebsiteLinksArgs(Id, Name, api, games, pluginName)
        {
            ProgressMessage = Loc.progress_adding_single_website_links(),
            ResultMessageId = LocId.dialog_added_links_message,
            DoForAllType = DoForAllTypes.SingleBlockingMultiBackground
        };
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

        if (gameName.IsNullOrEmpty())
        {
            return null;
        }

        switch (AddType)
        {
            case LinkAddTypes.UrlMatch:
                return gameName;

            case LinkAddTypes.SingleSearchResult:
                if (!CanBeSearched)
                {
                    return null;
                }

                var baseName = gameName.RemoveEditionSuffix();

                return await TryToFindPerfectMatchingUrl(game, gameName)
                    ?? (baseName == gameName ? await TryToFindPerfectMatchingUrl(game, baseName) : default);

            case LinkAddTypes.None:
                return null;

            default:
                return null;
        }
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

    public override async Task<bool> PrepareAsync(BaseActionArgs args)
    {
        await base.PrepareAsync(args);

        if (Pipeline != null)
        {
            return true;
        }

        Pipeline = new Pipeline(-1);

        return true;
    }

    public override bool ProcessUpdateData(Game gameToUpdate, BaseActionGame processedGame)
        => LinkHelper.UpdateGameInLibrary(gameToUpdate, processedGame);

    /// <summary>
    /// Opens a browser with the browser search url.
    /// </summary>
    /// <param name="game">Game the link will be searched for</param>
    /// <returns>True, if the browser could be opened</returns>
    public virtual bool StartBrowserSearch(Game game)
    {
        var url = GetBrowserSearchLink(game);

        if (url.IsNullOrEmpty())
        {
            return false;
        }

        var process = Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });

        return process is not null;
    }

    internal async Task<(bool Result, string? PageText)> LoadDocumentAsync(string url, string checkForContent = "", bool ignoreStatus = false)
    {
        if (Pipeline is null)
        {
            return (false, null);
        }

        var urlLoadResult = await Pipeline.LoadUrlAsync(url, DocumentType.Source, LinkUtilitiesPlugin.Settings.DebugMode, checkForContent, AllowedCallbackUrls);

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