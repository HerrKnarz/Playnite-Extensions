using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

public class LinkGamePressureGuides(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    public static string ClassId => $"linkutilities.gamepressure.guides.link";
    public override HashSet<string> AllowedCallbackUrls => ["https://www.gamepressure.com/guides/"];
    public override string BaseUrl => "https://www.gamepressure.com/";
    public override string LinkName => "gamepressure Guides";
    public override bool ReturnsSameUrl => true;

    public override List<TestCase> TestCases =>
    [
        new TestCase(){
            CaseName = "gamepressure Guides Cyberpunk 2077",
            GameName = "Cyberpunk 2077",
            GamePathExpected = "cyberpunk-2077/",
            SearchedUrlExpected = "not found!",
            UrlExpected = "https://www.gamepressure.com/cyberpunk-2077/"
        }
    ];

    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
        => (gameName ?? game.Name).RemoveDiacritics()
            .RemoveSpecialChars()?
            .Replace("_", " ")
            .CollapseWhitespaces()?
            .Replace(" ", "-")
            .ToLower() + '/';
}