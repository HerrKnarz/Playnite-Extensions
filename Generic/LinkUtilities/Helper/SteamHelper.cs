using LinkUtilities.LinkActions;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LinkUtilities.Helper
{
    internal static class SteamHelper
    {
        private static readonly string _steamAppPrefix = "steam://openurl/";
        private static readonly Regex _steamLinkRegex = new Regex(@"https?:\/\/(?:store\.steampowered|steamcommunity)\.com\/app\/(\d+)", RegexOptions.None);

        public static Guid SteamId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");

        internal static string GetSteamId(Game game)
        {
            if (game.PluginId == SteamId)
            {
                return game.GameId;
            }

            if (!string.IsNullOrEmpty(AddWebsiteLinks.Instance().SteamId))
            {
                return AddWebsiteLinks.Instance().SteamId;
            }

            if (game?.Links == null || !game.Links.Any())
            {
                return string.Empty;
            }

            var steamLink = game.Links.FirstOrDefault(l => _steamLinkRegex.Match(l.Url).Success);

            return steamLink == null ? string.Empty : GetSteamIdFromUrl(steamLink.Url);
        }

        internal static bool ChangeSteamLinks(Game game, bool toStoreLink = false, bool updateDb = false)
        {
            if (!game.Links?.Any() ?? true)
            {
                return false;
            }

            var mustUpdate = false;

            foreach (var link in game.Links)
            {
                if (!_steamLinkRegex.Match(link.Url).Success)
                {
                    continue;
                }

                var url = ChangeSteamLink(link.Url, toStoreLink);

                if (url == link.Url)
                {
                    continue;
                }

                if (GlobalSettings.Instance().OnlyATest)
                {
                    link.Url = url;
                }
                else
                {
                    API.Instance.MainView.UIDispatcher.Invoke(delegate
                    {
                        link.Url = url;
                    });
                }

                mustUpdate = true;
            }

            if (!mustUpdate)
            {
                return false;
            }

            if (updateDb && !GlobalSettings.Instance().OnlyATest)
            {
                API.Instance.Database.Games.Update(game);
            }

            return true;
        }

        internal static string GetSteamIdFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return string.Empty;
                }

                var linkMatch = _steamLinkRegex.Match(url);

                return linkMatch.Success ? linkMatch.Groups[1].Value : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        internal static string ChangeSteamLink(string url, bool toStoreLink = false)
        {
            return toStoreLink && url.StartsWith("http")
                ? _steamAppPrefix + url
                : !toStoreLink && url.StartsWith(_steamAppPrefix)
                ? url.Replace(_steamAppPrefix, string.Empty)
                : url;
        }
    }
}
