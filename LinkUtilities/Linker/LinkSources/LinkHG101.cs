using AngleSharp;
using AngleSharp.Html.Dom;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using System.Net;

namespace LinkUtilities.Linker.LinkSources;

internal class LinkHG101(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
    public static string ClassId => $"linkutilities.hardcoregaming101.link";
    public override string BaseUrl => "https://www.hardcoregaming101.net/";
    public override string LinkName => "Hardcore Gaming 101";
    public override string SearchUrl => $"{BaseUrl}?s=";

    public override List<TestCase> TestCases =>
        [
        new TestCase(){
            CaseName = "Hardcore Gaming 101 Metal Gear Solid 3: Snake Eater",
            GameName = "Metal Gear Solid 3: Snake Eater",
            GamePathExpected = "metal-gear-solid-3-snake-eater",
            SearchedUrlExpected = "https://www.hardcoregaming101.net/metal-gear-solid-3-snake-eater/",
            UrlExpected = "https://www.hardcoregaming101.net/metal-gear-solid-3-snake-eater"
        }
    ];

    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
        => (gameName ?? game.Name)
            .RemoveSpecialChars()
            .CollapseWhitespaces()?
            .Replace(" ", "-")
            .ToLower();

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

            var cells = document.QuerySelectorAll("header.entry-header");

            if (!cells.HasItems())
            {
                return await base.GetSearchResultsAsync(searchArgs);
            }

            var searchResults = new List<ChooseDialogItem>();

            foreach (var node in cells)
            {
                if (node.QuerySelector("a.Review") is null)
                {
                    continue;
                }

                if (node.QuerySelector("h2.entry-title > a") is not IHtmlAnchorElement link)
                {
                    continue;
                }

                var result = new LinkSearchResult
                {
                    Name = WebUtility.HtmlDecode(link.TextContent),
                    Url = link.Href
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