using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

internal class LinkGamerGuides(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    public static string ClassId => $"linkutilities.gamerguides.link";
    public override string BaseUrl => "https://www.gamerguides.com/";
    public override string LinkName => "Gamer Guides";
    public override bool ReturnsSameUrl => true;

    public override List<TestCase> TestCases =>
    [
        new TestCase(){
            CaseName = "Gamer Guides Guides Cyberpunk 2077",
            GameName = "Cyberpunk 2077",
            GamePathExpected = "cyberpunk-2077",
            SearchedUrlExpected = "not found!",
            UrlExpected = "https://www.gamerguides.com/cyberpunk-2077"
        }
    ];

    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
        => (gameName ?? game.Name)
            .RemoveSpecialChars()?
            .Replace("_", " ")
            .CollapseWhitespaces()?
            .Replace(" ", "-")
            .ToLower();
}