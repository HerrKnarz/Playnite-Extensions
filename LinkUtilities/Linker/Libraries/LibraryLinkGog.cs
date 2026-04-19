using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using Playnite;
using PlayniteCommon;
using PlayniteCommon.MetadataCommon;
using PlayniteCommon.WebCommon;

namespace LinkUtilities.Linker.Libraries;

internal class LibraryLinkGog : LibraryLinker
{
    public LibraryLinkGog() : base()
    {
        AllowedCallbackUrls.Add("https://www.gog.com/games");
    }

    public override bool AllowRedirects { get; set; } = false;
    public override string BaseUrl => "https://www.gog.com/en/game/";
    public override string BrowserSearchUrl => "https://www.gog.com/en/games?query=";
    public override string ExternalIdType => "gog";

    public IHttpClient HttpClient
    {
        get
        {
            field ??= new HttpClientWrapper(accept: "application/json");

            return field;
        }
    }

    /// <summary>
    /// ID of the game library to identify it in Playnite.
    /// </summary>
    public override string Id { get; } = LinkHelper.GogId;

    public override string LinkName => "GOG";
    public override bool ReturnsSameUrl { get; set; } = true;
    public override string SearchUrl => "https://catalog.gog.com/v1/catalog?limit=100&locale=en&order=desc:score&page=1&productType=in:game,pack&query=like:";

    public override async Task<(List<WebLink> links, bool result)> FindLibraryLinkAsync(Game game, string? gameId)
    {
        var links = new List<WebLink>();

        if (LinkUtilitiesPlugin.PlayniteApi is null)
        {
            return (links, false);
        }

        var typeId = await LibraryObjectHelper.GetLibraryObjectAsync(LinkUtilitiesPlugin.PlayniteApi.Library.WebLinkTypes, LinkName);

        if (typeId is null)
        {
            return (links, false);
        }

        if (LinkHelper.LinkExists(game, typeId.Id))
        {
            return (links, false);
        }

        gameId ??= game.LibraryGameId;

        var gogMetaData = await ApiHelper.GetJsonFromApiAsync<GogMetaData>(HttpClient, $"https://api.gog.com/products/{gameId}", LinkName);

        if (gogMetaData is null || gogMetaData.Slug.IsNullOrEmpty())
        {
            return (links, false);
        }

        LinkUrl = $"{BaseUrl}{gogMetaData.Slug}";

        links.Add(new WebLink(typeId.Id, LinkUrl));

        return (links, true);
    }

    // GOG Links need the game name in lowercase without special characters and underscores instead
    // of white spaces.
    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
        => (gameName ?? game.Name).RemoveDiacritics()
            .RemoveSpecialChars()
            .CollapseWhitespaces()?
            .Replace("-", "")
            .Replace(" ", "_")
            .ToLower();

    public override async Task<IEnumerable<ChooseDialogItem>> GetSearchResultsAsync(ChooseItemWithSearchAsyncArgs searchArgs)
    {
        var gogSearchResult = await ApiHelper.GetJsonFromApiAsync<GogSearchResult>(HttpClient, $"{SearchUrl}{searchArgs.SearchTerm.RemoveDiacritics().UrlEncode()}", LinkName);

        var searchResults = new List<ChooseDialogItem>();

        if (gogSearchResult is null || !gogSearchResult.Products.HasItems())
        {
            return searchResults;
        }

        foreach (var product in gogSearchResult.Products)
        {
            searchResults.Add(
                new LinkSearchResult
                {
                    Name = product.Title,
                    Url = product.StoreLink,
                    Description = $"{product.ReleaseDate} -  ID {product.Id}",
                    Id = product.Id
                });
        }

        return searchResults;
    }
}