using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

public class LinkEpic(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private const string _baseUrl = "https://store.epicgames.com/";
    public static string ClassId => $"linkutilities.epic.link";
    public override string BaseUrl => $"{_baseUrl}p/";
    public override string BrowserSearchUrl => $"{_baseUrl}en-US/browse?q=";
    public override string LinkName => "Epic";
    public override string SearchUrl => _baseUrl + "graphql?query={Catalog{searchStore(keywords:%22{SearchString}%22,category:%22games/edition%22,effectiveDate:%22[1900-01-01,{DateUntil}]%22,count:100){elements{title%20productSlug%20seller{name}}}}}";

    public override List<TestCase> TestCases =>
    [
        new TestCase(){
            CaseName = "Epic The Last Campfire",
            GameName = "The Last Campfire",
            GamePathExpected = "the-last-campfire",
            SearchedUrlExpected = "not found!",
            UrlExpected = "https://store.epicgames.com/p/the-last-campfire?lang=en-US"
        }
    ];

    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
    {
        var gamePath = (gameName ?? game.Name).RemoveDiacritics()
            .RemoveSpecialChars()
            .CollapseWhitespaces()?
            .Replace(" ", "-")
            .ToLower();

        AllowedCallbackUrls.Clear();
        AllowedCallbackUrls.Add($"{BaseUrl}{gamePath}?lang=en-US");

        return gamePath;
    }

    public override async Task<IEnumerable<ChooseDialogItem>> GetSearchResultsAsync(ChooseItemWithSearchAsyncArgs searchArgs)
    {
        if (Pipeline is null)
        {
            return await base.GetSearchResultsAsync(searchArgs);
        }

        try
        {
            // We use replace instead of format, because the URL already contains several braces.
            var url = SearchUrl
                .Replace("{SearchString}", searchArgs.SearchTerm.UrlEncode())
                .Replace("{DateUntil}", DateTime.Now.AddDays(1000).ToString("yyyy-MM-dd"));

            var epicSearchResult = await Pipeline.GetJsonFromApiAsync<EpicSearchResult>(url, LinkName, LinkUtilitiesPlugin.Settings.DebugMode);

            var validResults = epicSearchResult?.Data?.Catalog?.SearchStore?.Elements?.Where(n => !n.Title.IsNullOrEmpty() && !n.ProductSlug.IsNullOrEmpty()).ToList();

            return !validResults.HasItems()
                ? await base.GetSearchResultsAsync(searchArgs)
                : validResults.Select(n => new LinkSearchResult
                {
                    Name = n.Title,
                    Url = $"{BaseUrl}{n.ProductSlug}",
                    Description = n.Seller?.Name
                });
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error loading data from {LinkName}");
        }

        return await base.GetSearchResultsAsync(searchArgs);
    }
}