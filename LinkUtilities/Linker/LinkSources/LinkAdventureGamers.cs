using AngleSharp;
using AngleSharp.Html.Dom;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using System.Net;

namespace LinkUtilities.Linker.LinkSources;

public class LinkAdventureGamers(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
    public static string ClassId => $"linkutilities.adventuregamers.link";
    public override string BaseUrl => "https://adventuregamers.com/games/";
    public override string ExternalIdType => "adventure.gamers";
    public override string LinkName => "Adventure Gamers";
    public override string SearchUrl => "https://adventuregamers.com/?s=";

    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars()
                .CollapseWhitespaces()?
                .Replace(" ", "-")
                .ToLower();

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

            var cells = document.QuerySelectorAll(".search-results a.block");

            if (!cells.HasItems())
            {
                return await base.GetSearchResultsAsync(searchArgs);
            }

            var searchResults = new List<ChooseDialogItem>();

            foreach (var node in cells)
            {
                if (node is not IHtmlAnchorElement anchorNode || (!anchorNode.Href?.StartsWith(BaseUrl) ?? true))
                {
                    continue;
                }

                var result = new LinkSearchResult
                {
                    Name = WebUtility.HtmlDecode(node.TextContent.CollapseWhitespaces()?.Trim()),
                    Url = anchorNode.Href
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