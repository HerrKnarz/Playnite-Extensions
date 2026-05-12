using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

public class LinkIgn(string id, LinkSourceArgs args) : BaseLinkSource(id, args)
{
    public static string ClassId => $"linkutilities.ign.link";
    public override string BaseUrl => "https://www.ign.com/games/";
    public override string LinkName => "IGN";

    public override List<TestCase> TestCases =>
    [
        new TestCase(){
            CaseName = "IGN Monkey Island 2: LeChuck's Revenge",
            GameName = "Monkey Island 2: LeChuck's Revenge",
            GamePathExpected = "monkey-island-2-lechucks-revenge",
            SearchedUrlExpected = "not found!",
            UrlExpected = "https://www.ign.com/games/monkey-island-2-lechucks-revenge"
        }
    ];

    // IGN Links need the result name in lowercase without special characters and hyphens instead of
    // white spaces.
    public override async Task<string?> GetGamePathAsync(Game game, string? gameName = null)
        => (gameName ?? game.Name).RemoveDiacritics()
            .RemoveSpecialChars()?
            .Replace("_", " ")?
            .CollapseWhitespaces()?
            .Replace(" ", "-")
            .ToLower();
}