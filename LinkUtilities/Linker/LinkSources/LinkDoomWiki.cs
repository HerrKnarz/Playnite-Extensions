using LinkUtilities.Helper;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

public class LinkDoomWiki(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    private const string _baseUrl = "https://doomwiki.org/";
    public static string ClassId => $"linkutilities.doomwiki.link";
    public override string BaseUrl => $"{_baseUrl}wiki/";
    public override string BrowserSearchUrl => $"{_baseUrl}w/index.php?title=Special%3ASearch&profile=default&fulltext=Search&search=";
    public override string LinkName => "Doom Wiki";
    public override string SearchUrl => $"{_baseUrl}w/api.php?action=opensearch&format=xml&search={{0}}&limit=50";

    public override List<TestCase> TestCases =>
    [
        new TestCase(){
            CaseName = "Doom Wiki Doom",
            GameName = "Doom (2016)",
            GamePathExpected = "Doom_%282016%29",
            SearchedUrlExpected = "not found!",
            UrlExpected = "https://doomwiki.org/wiki/Doom_%282016%29"
        }
    ];

    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
        => (gameName ?? game.Name).CollapseWhitespaces()?
            .Replace(" ", "_")
            .EscapeDataString();

    public override async Task<IEnumerable<ChooseDialogItem>> GetSearchResultsAsync(ChooseItemWithSearchAsyncArgs searchArgs)
        => [.. await ParseHelper.GetMediaWikiResultsFromApiAsync(SearchUrl, searchArgs.SearchTerm, LinkName, this)];
}