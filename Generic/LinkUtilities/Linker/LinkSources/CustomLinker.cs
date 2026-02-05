using KNARZhelper;
using KNARZhelper.WebCommon;
using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using LinkUtilities.Models;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LinkUtilities.Linker.LinkSources
{
    public class CustomLinker : BaseClasses.Linker
    {
        private readonly string _placeholderGameName = "{GameName}";
        private readonly string _placeholderGogId = "{GogId}";
        private readonly string _placeholderRomName = "{RomName}";
        private readonly string _placeholderSteamId = "{SteamId}";

        public CustomLinker(CustomLinkProfile customSettings)
        {
            CustomSettings = customSettings;

            Settings.IsCustomSource = true;
        }

        public override LinkAddTypes AddType => CustomSettings?.GameUrl?.Any() ?? false ? LinkAddTypes.UrlMatch : LinkAddTypes.None;
        public override bool AllowRedirects => CustomSettings?.AllowRedirects ?? true;
        public override string BaseUrl => CustomSettings?.GameUrl ?? string.Empty;
        public override string BrowserSearchUrl => CustomSettings?.BrowserSearchUrl ?? string.Empty;
        public CustomLinkProfile CustomSettings { get; }
        public override string LinkName => CustomSettings?.Name ?? string.Empty;
        public override bool NeedsToBeChecked => CustomSettings?.NeedsToBeChecked ?? true;
        public override int Priority => BaseUrl.Contains(_placeholderSteamId) ? 10 : 1;
        public override bool ReturnsSameUrl => CustomSettings?.ReturnsSameUrl ?? false;
        public override UrlLoadMethod UrlLoadMethod => UrlLoadMethod.NewDefault;

        public override bool FindLinks(Game game, out List<Link> links)
        {
            LinkUrl = string.Empty;
            links = new List<Link>();

            if (LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            var linkUrl = ReplacePlaceholders(BaseUrl, game);

            if (string.IsNullOrEmpty(linkUrl))
            {
                return false;
            }

            if (!NeedsToBeChecked || CheckLink(linkUrl))
            {
                LinkUrl = linkUrl;
            }

            if (string.IsNullOrEmpty(LinkUrl))
            {
                return false;
            }

            links.Add(new Link(LinkName, LinkUrl));

            return true;
        }

        public override string GetBrowserSearchLink(Game game = null) => ReplacePlaceholders(BrowserSearchUrl, game, true);

        public override string GetGamePath(Game game, string gameName = null) => CustomSettings.FormatGameName(game.Name);

        private string ReplacePlaceholders(string url, Game game, bool forBrowserSearch = false)
        {
            var result = url;

            if (result.Contains(_placeholderGameName))
            {
                var gameName = forBrowserSearch ? game.Name.UrlEncode() : GetGamePath(game);

                if (string.IsNullOrEmpty(gameName))
                {
                    return string.Empty;
                }

                result = result.Replace(_placeholderGameName, gameName);
            }

            if (result.Contains(_placeholderSteamId))
            {
                var steamId = GetSteamId(game);

                if (string.IsNullOrEmpty(steamId))
                {
                    return string.Empty;
                }

                result = result.Replace(_placeholderSteamId, steamId);
            }

            if (result.Contains(_placeholderGogId))
            {
                if (game.PluginId != LinkHelper.GogId)
                {
                    return string.Empty;
                }

                result = result.Replace(_placeholderGogId, game.GameId);
            }

            if (result.Contains(_placeholderRomName))
            {
                if (game.IsInstalled && (game.Roms?.Any() ?? false))
                {
                    result = result.Replace(_placeholderRomName, Path.GetFileNameWithoutExtension(game.Roms[0].Path));
                }
                else
                {
                    return string.Empty;
                }
            }

            return result;
        }
    }
}