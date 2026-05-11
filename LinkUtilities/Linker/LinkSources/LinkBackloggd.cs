using AngleSharp;
using AngleSharp.Html.Dom;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.WebCommon;
using System.Net;

namespace LinkUtilities.Linker.LinkSources;

public class LinkBackloggd(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private const string _baseUrl = "https://backloggd.com";
    private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
    public static string ClassId => $"linkutilities.backloggd.link";
    public override string BaseUrl => $"{_baseUrl}/games/";
    public override string ExternalIdType => "backloggd";
    public override string LinkName => "Backloggd";
    public override string SearchUrl => $"{_baseUrl}/search/games/";

    public override List<TestCase> TestCases =>
        [
            new TestCase(){
                CaseName = "Backloggd Monkey Island 2: LeChuck's Revenge",
                GameName = "Monkey Island 2: LeChuck's Revenge",
                GamePathExpected = "monkey-island-2-lechucks-revenge",
                SearchedUrlExpected = "not found!",
                UrlExpected = "https://backloggd.com/games/monkey-island-2-lechucks-revenge"
            }
        ];

    // Since Backloggd always returns the status code OK and the same url, even if that leads to a
    // non-existing game, we also check if the title isn't the one of that generic page. Funny
    // enough it says 404 but doesn't return that status code...
    public override string WrongTitle => "404 Not Found";

    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
        => (gameName ?? game.Name).RemoveSpecialChars()
            .CollapseWhitespaces()?
            .Replace(" ", "-")
            .ToLower();

    public override async Task<IEnumerable<ChooseDialogItem>> GetSearchResultsAsync(ChooseItemWithSearchAsyncArgs searchArgs)
    {
        try
        {
            var (statusOk, htmlSource) = await LoadDocumentAsync($"{SearchUrl}{searchArgs.SearchTerm.UrlEncode()}", "<div class=\"col-12 result\"", true, 100);

            if (!statusOk || htmlSource.IsNullOrEmpty())
            {
                return await base.GetSearchResultsAsync(searchArgs);
            }

            var document = await _context.OpenAsync(req => req.Content(htmlSource));

            var cells = document.QuerySelectorAll("div.result");

            if (!cells.HasItems())
            {
                return await base.GetSearchResultsAsync(searchArgs);
            }

            var searchResults = new List<ChooseDialogItem>();

            foreach (var node in cells)
            {
                var nameHtml = (IHtmlAnchorElement?)node.QuerySelector("div.game-name > a");

                if (nameHtml is null)
                {
                    continue;
                }

                var platformHtml = node.QuerySelector("div.search-result-platforms");
                var description = nameHtml.QuerySelector("span.subtitle-text")?.TextContent;

                if (platformHtml is not null)
                {
                    var yearHtml = nameHtml.QuerySelector("span.subtitle-text");

                    if (yearHtml != null)
                    {
                        description = WebUtility.HtmlDecode(yearHtml.TextContent) + " - ";
                    }

                    var typeHtml = platformHtml.QuerySelector("p");

                    if (typeHtml != null)
                    {
                        description += WebUtility.HtmlDecode(typeHtml.TextContent) + " - ";
                    }

                    description += Loc.GetString("platforms_title") + ": ";

                    description += string.Join(", ", platformHtml.QuerySelectorAll("span > p").Select(p => p.TextContent));
                }

                var result = new LinkSearchResult
                {
                    Name = WebUtility.HtmlDecode(nameHtml.QuerySelector("h3")?.GetDirectText()),
                    Url = $"{_baseUrl}{nameHtml.PathName}",
                    Description = description
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