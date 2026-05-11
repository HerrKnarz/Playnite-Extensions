using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

public class LinkGameFaqs(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    public static string ClassId => $"linkutilities.gamefaqs.link";
    public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
    public override string BaseUrl => "https://gamefaqs.gamespot.com";
    public override string BrowserSearchUrl => $"{BaseUrl}/search?game=";
    public override string ExternalIdType => "gamefaqs";
    public override string LinkName => "GameFAQs";
    public override string SearchUrl => $"{BaseUrl}/ajax/home_game_search?term=&term=";

    public override List<TestCase> TestCases =>
    [
        new TestCase(){
            CaseName = "GameFAQs Monkey Island 2: LeChuck's Revenge",
            GameName = "Monkey Island 2: LeChuck's Revenge",
            GamePathExpected = "https://gamefaqs.gamespot.com/amiga/584295-monkey-island-2-lechucks-revenge",
            SearchedUrlExpected = "not found!",
            UrlExpected = "https://gamefaqs.gamespot.com/amiga/584295-monkey-island-2-lechucks-revenge"
        }
    ];

    public override async Task<IEnumerable<ChooseDialogItem>> GetSearchResultsAsync(ChooseItemWithSearchAsyncArgs searchArgs)
    {
        if (Pipeline is null)
        {
            return await base.GetSearchResultsAsync(searchArgs);
        }

        try
        {
            var searchResults = await Pipeline.GetJsonFromApiAsync<List<GameFaqsSearchResult>>($"{SearchUrl}{searchArgs.SearchTerm.UrlEncode()}", LinkName, LinkUtilitiesPlugin.Settings.DebugMode);

            return searchResults?.Count == 0
                ? await base.GetSearchResultsAsync(searchArgs)
                : searchResults!.Where(n => n.GameName?.Length > 0).ToList().Select(n => new LinkSearchResult
                {
                    Name = n.GameName,
                    Url = $"{BaseUrl}{n.Url}",
                    Description = n.DateReleased,
                    Id = n.GameId
                });
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error loading data from {LinkName}");
        }

        return await base.GetSearchResultsAsync(searchArgs);
    }
}