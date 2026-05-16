using AngleSharp;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using System.Net;

namespace LinkUtilities.Linker.LinkSources;

internal class LinkGrouvee(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
    public static string ClassId => $"linkutilities.grouvee.link";
    public override LinkAddTypes AddType => LinkAddTypes.SingleSearchResult;
    public override string BaseUrl => "https://www.grouvee.com";
    public override string LinkName => "Grouvee";
    public override string SearchUrl => $"{BaseUrl}/search/?q=";

    public override List<TestCase> TestCases =>
        [
        new TestCase(){
            CaseName = "Grouvee Cyberpunk 2077",
            GameName = "Cyberpunk 2077",
            GamePathExpected = "https://www.grouvee.com/games/120722-cyberpunk-2077/",
            SearchedUrlExpected = "https://www.grouvee.com/games/120722-cyberpunk-2077/",
            UrlExpected = "https://www.grouvee.com/games/120722-cyberpunk-2077/"
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

            var cells = document.QuerySelectorAll("div.game-row > div");

            if (!cells.HasItems())
            {
                return await base.GetSearchResultsAsync(searchArgs);
            }

            var searchResults = new List<ChooseDialogItem>();

            foreach (var node in cells)
            {
                var platformNodes = node.QuerySelectorAll("div.platform-list > span");

                string? description = null;

                if (platformNodes.HasItems())
                {
                    description = string.Join(", ", platformNodes.Select(p => WebUtility.HtmlDecode(p.TextContent.Trim())));
                }

                var result = new LinkSearchResult
                {
                    Name = WebUtility.HtmlDecode(node.QuerySelector("h4 > a")?.TextContent),
                    Description = description,
                    Url = BaseUrl + node.QuerySelector("h4 > a")?.GetAttribute("href")
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