using Playnite;
using PlayniteExtensionHelpers;

namespace LinkUtilities.Linker.LinkSources;

internal class LinkIgn : BaseClasses.Linker
{
    public override string BaseUrl => "https://www.ign.com/games/";
    public override string LinkName => "IGN";

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