using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.GamesCommon;
using PlayniteExtensionHelpers.MetadataCommon;
using PlayniteExtensionHelpers.WebCommon;
using System.Diagnostics;
using System.Net;

namespace LinkUtilities.BaseClasses;

/// <summary>
/// Base class for a website link
/// </summary>
public abstract class Linker : BaseAction, ILinker
{
    public virtual bool AcceptSingleSearchResult => true;

    public virtual LinkAddTypes AddType => LinkAddTypes.UrlMatch;
    public virtual HashSet<string> AllowedCallbackUrls { get; set; } = [];
    public virtual bool AllowRedirects { get; set; } = true;
    public virtual string BaseUrl => string.Empty;
    public virtual string BrowserSearchUrl => SearchUrl;
    public virtual bool CanBeBrowserSearched => !BrowserSearchUrl.IsNullOrWhiteSpace();
    public virtual bool CanBeSearched => !SearchUrl.IsNullOrWhiteSpace();
    public virtual string CheckForContent { get; set; } = string.Empty;
    public virtual int Delay => 0;
    public virtual string? ExternalIdType => null;
    public override string Id => $"linkutilities.{ExternalIdType}.link";
    public bool Initialized { get; set; } = false;
    public abstract string LinkName { get; }
    public virtual string LinkTypeId { get; set; } = string.Empty;
    public virtual string LinkUrl { get; set; } = string.Empty;
    public override string Name => $"{LinkName} link";
    public virtual bool NeedsToBeChecked { get; set; } = true;

    public virtual Pipeline? Pipeline { get; set; }

    public virtual int Priority => 1;

    public virtual bool ReturnsSameUrl { get; set; } = false;

    public virtual string SearchUrl => string.Empty;

    public LinkSourceSetting Settings { get; set; } = new();

    public virtual string WrongTitle { get; set; } = string.Empty;

    public virtual async Task<bool> AddLinkAsync(Game game)
    {
        var result = await FindLinksAsync(game);

        return result.result && await LinkHelper.AddLinksAsync(game, result.links);
    }

    public virtual async Task<bool> AddLinkFromSearchAsync(Game game, LinkSearchResult result, bool cleanUpAfterAdding = true)
    {
        if (!result.Id.IsNullOrEmpty())
        {
            await LinkHelper.AddExternalIdAsync(game, ExternalIdType, result.Id, LinkName);
        }

        return await LinkHelper.AddLinkAsync(game, LinkName, result.Url, LinkTypeId, false, cleanUpAfterAdding);
    }

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

    public virtual async Task<bool> CheckLinkAsync(string link)
        => Pipeline is not null
        && await Pipeline.IsUrlOkAsync(link, ReturnsSameUrl, WrongTitle, LinkUtilitiesPlugin.Settings.DebugMode, CheckForContent, AllowedCallbackUrls);

    public override async Task<bool> ExecuteAsync(GameEx game, BaseActionArgs args)
    {
        if (args is not AddWebsiteLinksArgs addArgs)
        {
            return false;
        }

        async Task<bool> AddLinks()
        {
            if (addArgs.IsBulkAction && (Delay > 0))
            {
                await Task.Delay(Delay);
            }

            return await AddLinkAsync(game.Game);
        }

        return addArgs.AddType switch
        {
            AddWebsiteLinkTypes.Add
            or AddWebsiteLinkTypes.AddSelected
                => await AddLinks(),
            AddWebsiteLinkTypes.Search
            or AddWebsiteLinkTypes.SearchSelected
                => await AddSearchedLinkAsync(game.Game),
            AddWebsiteLinkTypes.SearchMissing
                => await AddSearchedLinkAsync(game.Game, true),
            AddWebsiteLinkTypes.SearchInBrowser
                => StartBrowserSearch(game.Game),
            _ => throw new ArgumentOutOfRangeException(nameof(args), addArgs.AddType, null),
        };
    }

    public virtual async Task<(List<WebLink> links, bool result)> FindLinksAsync(Game game)
    {
        LinkUrl = string.Empty;

        var links = new List<WebLink>();

        if (LinkHelper.LinkExists(game, LinkTypeId))
        {
            return (links, false);
        }

        switch (AddType)
        {
            case LinkAddTypes.SingleSearchResult:
                LinkUrl = await GetGamePathAsync(game) ?? string.Empty;
                break;

            case LinkAddTypes.UrlMatch:
                var gameName = await GetGamePathAsync(game);

                if (gameName.IsNullOrEmpty())
                {
                    break;
                }

                if (!NeedsToBeChecked || await CheckLinkAsync($"{BaseUrl}{gameName}"))
                {
                    LinkUrl = $"{BaseUrl}{gameName}";
                }
                else
                {
                    var baseName = game.Name.RemoveEditionSuffix();

                    if (baseName == game.Name)
                    {
                        break;
                    }

                    gameName = await GetGamePathAsync(game, baseName);

                    if (!NeedsToBeChecked || await CheckLinkAsync($"{BaseUrl}{gameName}"))
                    {
                        LinkUrl = $"{BaseUrl}{gameName}";
                    }
                }

                break;

            case LinkAddTypes.None:
                break;

            default:
                break;
        }

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

    public override AddWebsiteLinksArgs GetActionArgs(IPlayniteApi api, List<GameEx> games, string pluginName)
    {
        return new AddWebsiteLinksArgs(Id, Name, api, games, pluginName)
        {
            ProgressMessage = Loc.progress_adding_single_website_links(),
            ResultMessageId = LocId.dialog_added_links_message
        };
    }

    public virtual string GetBrowserSearchLink(Game game) => BrowserSearchUrl + (game.Name.UrlEncode() ?? string.Empty);

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

    public virtual async Task<IEnumerable<ChooseDialogItem>> GetSearchResultsAsync(ChooseItemWithSearchAsyncArgs searchArgs) => [];

    public virtual async Task InitializeAsync()
    {
        if (Initialized)
        {
            return;
        }

        Settings.LinkName = LinkName;
        Settings.IsAddable = AddType != LinkAddTypes.None ? true : null;
        Settings.IsSearchable = CanBeSearched ? true : null;
        Settings.ShowInMenus = true;
        Settings.ApiKey = null;
        Settings.NeedsApiKey = false;

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

    internal static string? GetSteamId(Game game)
    {
        var steamId = SteamHelper.GetSteamId(game);

        return steamId.IsNullOrEmpty() ? AddWebsiteLinks.Instance().SteamId : steamId;
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