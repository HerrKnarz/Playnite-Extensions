using Playnite;
using System.Text.RegularExpressions;

namespace PlayniteExtensionHelpers;

public static partial class SteamHelper
{
    private static readonly string _steamAppPrefix = "steam://openurl/";
    public static string ExternalIdType => "steam";
    public static string SteamId => "Crow.Steam";

    /// <summary>
    /// Converts a Steam store/community link to a steam://openurl/ link and vice versa.
    /// </summary>
    /// <param name="url">The URL to convert.</param>
    /// <param name="toStoreLink">
    /// If true, converts to a store link; otherwise, converts to a steam://openurl/ link.
    /// </param>
    /// <returns>The converted URL.</returns>
    public static string ConvertSteamLink(string url, bool toStoreLink = false)
    {
        return toStoreLink && url.StartsWith("http")
            ? _steamAppPrefix + url
            : !toStoreLink && url.StartsWith(_steamAppPrefix)
            ? url.Replace(_steamAppPrefix, string.Empty)
            : url;
    }

    /// <summary>
    /// Converts all steam links of a game to a steam://openurl/ link and vice versa.
    /// </summary>
    /// <param name="game">The game for which to convert links.</param>
    /// <param name="toStoreLink">
    /// If true, converts to a store link; otherwise, converts to a steam://openurl/ link.
    /// </param>
    /// <param name="updateDb">If true, updates the game in the database.</param>
    /// <param name="api">The Playnite API instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task<bool> ConvertSteamLinksAsync(Game game, bool toStoreLink = false, bool updateDb = false, IPlayniteApi? api = null)
    {
        if (!game.Links.HasItems())
        {
            return false;
        }

        var mustUpdate = false;

        foreach (var link in game.Links)
        {
            if (link.Url.IsNullOrEmpty() || !SteamLinkRegex().IsMatch(link.Url))
            {
                continue;
            }

            var url = ConvertSteamLink(link.Url, toStoreLink);

            if (url == link.Url)
            {
                continue;
            }

            link.Url = url;

            mustUpdate = true;
        }

        if (!mustUpdate)
        {
            return false;
        }

        if (updateDb && api is not null)
        {
            await api.Library.Games.UpdateAsync(game);
        }

        return true;
    }

    /// <summary>
    /// Tries to determine the steam id of a game by checking the library id, external identifiers
    /// and links.
    /// </summary>
    /// <param name="game">The game for which to determine the steam id.</param>
    /// <returns>The steam id if found; otherwise, null.</returns>
    public static string? GetSteamId(Game game)
    {
        if (game.LibraryId == SteamId)
        {
            return game.LibraryGameId;
        }

        if (game.ExternalIdentifiers.HasItems())
        {
            var steamId = game.ExternalIdentifiers.FirstOrDefault(e => e.TypeId == ExternalIdType);

            if (steamId is not null)
            {
                return steamId.IdValue;
            }
        }

        if (!game.Links.HasItems())
        {
            return null;
        }

        var steamLink = game.Links.FirstOrDefault(l => !l.Url.IsNullOrEmpty() && SteamLinkRegex().IsMatch(l.Url));

        return steamLink is null ? null : GetSteamIdFromUrl(steamLink.Url);
    }

    /// <summary>
    /// Gets the steam id from a steam store/community link.
    /// </summary>
    /// <param name="url">The URL from which to extract the steam id.</param>
    /// <returns>The steam id if found; otherwise, null.</returns>
    public static string? GetSteamIdFromUrl(string? url)
    {
        try
        {
            if (url.IsNullOrEmpty())
            {
                return null;
            }

            var linkMatch = SteamLinkRegex().Match(url);

            return linkMatch.Success ? linkMatch.Groups[1].Value : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    [GeneratedRegex(@"https?:\/\/(?:store\.steampowered|steamcommunity)\.com\/app\/(\d+)", RegexOptions.None)]
    private static partial Regex SteamLinkRegex();
}