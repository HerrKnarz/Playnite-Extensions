using Playnite;

namespace PlayniteExtensionHelpers;

public static class GogHelper
{
    public static string ExternalIdType => "gog";
    public static string GogId => "Crow.GOG";

    /// <summary>
    /// Tries to determine the GOG id of a game by checking the library id and external identifiers.
    /// </summary>
    /// <param name="game">The game for which to determine the GOG id.</param>
    /// <returns>The GOG id if found; otherwise, null.</returns>
    public static string? GetGogId(Game game)
    {
        if (game.LibraryId == GogId)
        {
            return game.LibraryGameId;
        }

        if (game.ExternalIdentifiers.HasItems())
        {
            var gogId = game.ExternalIdentifiers.FirstOrDefault(e => e.TypeId == ExternalIdType);

            if (gogId is not null)
            {
                return gogId.IdValue;
            }
        }

        return null;
    }
}