using Playnite;
using System.Text.RegularExpressions;

namespace PlayniteCommon;

internal static partial class SteamHelper
{
    internal static string ExternalIdType = "steam";
    internal static string SteamId = "Crow.Steam";
    private static readonly string _steamAppPrefix = "steam://openurl/";
    private static readonly Regex _steamLinkRegex = SteamLinkRegex();

    internal static string ChangeSteamLink(string url, bool toStoreLink = false)
    {
        return toStoreLink && url.StartsWith("http")
            ? _steamAppPrefix + url
            : !toStoreLink && url.StartsWith(_steamAppPrefix)
            ? url.Replace(_steamAppPrefix, string.Empty)
            : url;
    }

    internal static async Task<bool> ConvertSteamLinksAsync(Game game, bool toStoreLink = false, bool updateDb = false, IPlayniteApi? api = null)
    {
        if (!game.Links.HasItems())
        {
            return false;
        }

        var mustUpdate = false;

        foreach (var link in game.Links)
        {
            if (link.Url.IsNullOrEmpty() || !_steamLinkRegex.IsMatch(link.Url))
            {
                continue;
            }

            var url = ChangeSteamLink(link.Url, toStoreLink);

            if (url == link.Url)
            {
                continue;
            }

            UIDispatcher.Invoke(delegate
            {
                link.Url = url;
            });

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

    internal static string? GetSteamId(Game game)
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
            return string.Empty;
        }

        var steamLink = game.Links.FirstOrDefault(l => !l.Url.IsNullOrEmpty() && _steamLinkRegex.IsMatch(l.Url));

        return steamLink is null ? null : GetSteamIdFromUrl(steamLink.Url);
    }

    internal static string? GetSteamIdFromUrl(string? url)
    {
        try
        {
            if (url.IsNullOrEmpty())
            {
                return null;
            }

            var linkMatch = _steamLinkRegex.Match(url);

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