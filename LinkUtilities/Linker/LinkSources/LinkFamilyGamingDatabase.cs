using AngleSharp;
using AngleSharp.Html.Dom;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using System.Net;

namespace LinkUtilities.Linker.LinkSources;

public class LinkFamilyGamingDatabase(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private const string _baseUrl = "https://www.familygamingdatabase.com/";
    private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
    public static string ClassId => $"linkutilities.familygamingdatabase.link";
    public override string BaseUrl => $"{_baseUrl}en-gb/game/";
    public override string CheckForContent => "<div class=\"gameTitleShare\"";
    public override int Delay => 200;
    public override string LinkName => "Family Gaming Database";
    public override string SearchUrl => _baseUrl + "search/text/{0}/type/video+games/sort/Name/";

    public override List<TestCase> TestCases =>
    [
        new TestCase(){
            CaseName = "Family Gaming Database The Last Campfire",
            GameName = "The Last Campfire",
            GamePathExpected = "The+Last+Campfire",
            SearchedUrlExpected = "not found!",
            UrlExpected = "https://www.familygamingdatabase.com/en-gb/game/The+Last+Campfire"
        }
    ];

    public override string GetBrowserSearchLink(Game? game = null) => string.Format(BrowserSearchUrl, game?.Name.UrlEncode());

    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
        => (gameName ?? game.Name)
            .RemoveSpecialChars()
            .CollapseWhitespaces()?
            .Replace(" ", "+");

    public override async Task<IEnumerable<ChooseDialogItem>> GetSearchResultsAsync(ChooseItemWithSearchAsyncArgs searchArgs)
    {
        try
        {
            var (statusOk, htmlSource) = await LoadDocumentAsync(string.Format(SearchUrl, searchArgs.SearchTerm.UrlEncode()));

            if (!statusOk || htmlSource.IsNullOrEmpty())
            {
                return await base.GetSearchResultsAsync(searchArgs);
            }

            var document = await _context.OpenAsync(req => req.Content(htmlSource));

            var cells = document.QuerySelectorAll("#searchResultsFlexContainer div.listItem");

            if (!cells.HasItems())
            {
                return await base.GetSearchResultsAsync(searchArgs);
            }

            var searchResults = new List<ChooseDialogItem>();

            foreach (var node in cells)
            {
                var linkNode = (IHtmlAnchorElement?)node.QuerySelector("div.sharingImage > ribbon > a");

                if (linkNode is null)
                { continue; }

                var result = new LinkSearchResult
                {
                    Name = WebUtility.HtmlDecode(linkNode.TextContent.CollapseWhitespaces()?.Trim()),
                    Url = linkNode.Href,
                    Description = WebUtility.HtmlDecode(node.QuerySelector("div.listViewOverview")?.TextContent.CollapseWhitespaces()?.Trim())
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