using AngleSharp;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using System.Net;

namespace LinkUtilities.Linker.LinkSources;

public class LinkMobyGames(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
    public static string ClassId => $"linkutilities.mobygames.link";
    public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
    public override string LinkName => "MobyGames";
    public override string SearchUrl => "https://www.mobygames.com/search/?type=game&q=";

    public override List<TestCase> TestCases =>
        [
        new TestCase(){
            CaseName = "MobyGames Dragon Age: Inquisition",
            GameName = "Dragon Age: Inquisition",
            GamePathExpected = "https://www.mobygames.com/game/70317/dragon-age-inquisition/",
            SearchedUrlExpected = "https://www.mobygames.com/game/70317/dragon-age-inquisition/",
            UrlExpected = "https://www.mobygames.com/game/70317/dragon-age-inquisition/"
        }
    ];

    public override async Task<IEnumerable<ChooseDialogItem>> GetSearchResultsAsync(ChooseItemWithSearchAsyncArgs searchArgs)
    {
        try
        {
            var (statusOk, htmlSource) = await LoadDocumentAsync($"{SearchUrl}{searchArgs.SearchTerm.UrlEncode()}");

            if (!statusOk || htmlSource.IsNullOrEmpty())
            {
                return await base.GetSearchResultsAsync(searchArgs);
            }

            var document = await _context.OpenAsync(req => req.Content(htmlSource));

            var cells = document.QuerySelectorAll("tbody >tr > td:last-of-type");

            if (!cells.HasItems())
            {
                return await base.GetSearchResultsAsync(searchArgs);
            }

            var searchResults = new List<ChooseDialogItem>();

            foreach (var node in cells)
            {
                var result = new LinkSearchResult
                {
                    Name = WebUtility.HtmlDecode(node.QuerySelector("b a")?.TextContent),
                    Description = WebUtility.HtmlDecode(node.QuerySelector("small:last-of-type")?.TextContent.CollapseWhitespaces()),
                    Url = node.QuerySelector("b a")?.GetAttribute("href")
                };

                if (result.Name.IsNullOrEmpty() || result.Url.IsNullOrEmpty())
                {
                    continue;
                }

                searchResults.Add(result);
            }

            return searchResults;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error loading data from {LinkName}");
        }

        return await base.GetSearchResultsAsync(searchArgs);
    }
}