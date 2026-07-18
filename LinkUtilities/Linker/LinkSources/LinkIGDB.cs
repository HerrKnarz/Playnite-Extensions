using AngleSharp;
using AngleSharp.Html.Dom;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using System.Net;

namespace LinkUtilities.Linker.LinkSources;

public class LinkIGDB(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private const string _websiteUrl = "https://www.igdb.com/";
    private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
    private string? _gameSlug = string.Empty;
    public static string ClassId => $"linkutilities.igdb.link";
    public override string BaseUrl => $"{_websiteUrl}games/";
    public override string CheckForContent => $"<meta content=\"{BaseUrl}{_gameSlug}\"";
    public override string LinkName => "IGDB";
    public override string SearchUrl => $"{_websiteUrl}search?utf8=✓&q=";

    public override List<TestCase> TestCases =>
        [
        new TestCase(){
            CaseName = "IGDB Metal Gear Solid 3: Snake Eater",
            GameName = "Metal Gear Solid 3: Snake Eater",
            GamePathExpected = "metal-gear-solid-3-snake-eater",
            SearchedUrlExpected = "https://www.igdb.com/games/metal-gear-solid-3-snake-eater",
            UrlExpected = "https://www.igdb.com/games/metal-gear-solid-3-snake-eater"
        }
    ];

    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
    {
        _gameSlug = (gameName ?? game.Name)
                .RemoveDiacritics()
                .RemoveSpecialChars()
                .CollapseWhitespaces()?
                .Replace(" ", "-")
                .ToLower();

        return _gameSlug;
    }

    public override async Task<IEnumerable<ChooseDialogItem>> GetSearchResultsAsync(ChooseItemWithSearchAsyncArgs searchArgs)
    {
        try
        {
            var (statusOk, htmlSource) = await LoadDocumentAsync($"{SearchUrl}{searchArgs.SearchTerm.UrlEncode()}", string.Empty, true);

            if (!statusOk || htmlSource.IsNullOrEmpty())
            {
                return await base.GetSearchResultsAsync(searchArgs);
            }

            var document = await _context.OpenAsync(req => req.Content(htmlSource));

            var cells = document.QuerySelectorAll("#search-results .media-body");

            if (!cells.HasItems())
            {
                return await base.GetSearchResultsAsync(searchArgs);
            }

            var searchResults = new List<ChooseDialogItem>();

            foreach (var node in cells)
            {
                if (node.QuerySelector("a") is not IHtmlAnchorElement link)
                {
                    continue;
                }

                var result = new LinkSearchResult
                {
                    Name = WebUtility.HtmlDecode(link.TextContent),
                    Url = link.Href,
                    Description = WebUtility.HtmlDecode(node.QuerySelector(".mar-md-bottom a")?.TextContent.CollapseWhitespaces()),
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