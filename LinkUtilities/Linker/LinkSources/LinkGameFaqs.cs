using LinkUtilities.Interfaces;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using Playnite;
using PlayniteCommon;

namespace LinkUtilities.Linker.LinkSources;

internal class LinkGameFaqs : BaseClasses.Linker
{
    public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
    public override string BaseUrl => "https://gamefaqs.gamespot.com/";
    public override string BrowserSearchUrl => $"{BaseUrl}search?game=";
    public override string ExternalIdType => "gamefaqs";
    public override string LinkName => "GameFAQs";
    public override string SearchUrl => $"{BaseUrl}ajax/home_game_search?term=&term=";

    public override async Task<IEnumerable<ChooseDialogItem>> GetSearchResultsAsync(ChooseItemWithSearchAsyncArgs searchArgs)
    {
        if (Pipeline is null)
        {
            return await base.GetSearchResultsAsync(searchArgs);
        }

        try
        {
            var searchResults = await Pipeline.GetJsonFromApiAsync<List<GameFaqsSearchResult>>($"{SearchUrl}{searchArgs.SearchTerm.UrlEncode()}", LinkName, LinkUtilitiesPlugin.Settings.DebugMode);

            if (searchResults?.Count == 0)
            {
                if (LinkUtilitiesPlugin.Settings.DebugMode)
                {
                    Log.Info($"No results found for {searchArgs.SearchTerm} from {LinkName}");
                }

                return await base.GetSearchResultsAsync(searchArgs);
            }

            return searchResults!.Where(n => n.GameName?.Length > 0).ToList().Select(n => new LinkSearchResult
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