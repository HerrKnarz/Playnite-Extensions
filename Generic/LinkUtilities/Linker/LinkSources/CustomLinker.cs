using KNARZhelper;
using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using LinkUtilities.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LinkUtilities.Linker.LinkSources
{
    public class CustomLinker : BaseClasses.Linker
    {
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
        public override bool ReturnsSameUrl => CustomSettings?.ReturnsSameUrl ?? false;

        public override bool FindLinks(Game game, out List<Link> links)
        {
            LinkUrl = string.Empty;
            links = new List<Link>();

            if (LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            var linkUrl = BaseUrl;

            if (linkUrl.Contains("{GameName}"))
            {
                var gameName = GetGamePath(game);

                if (string.IsNullOrEmpty(gameName))
                {
                    return false;
                }

                linkUrl = linkUrl.Replace("{GameName}", gameName);
            }

            if (linkUrl.Contains("{SteamId}"))
            {
                if (game.PluginId == Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab"))
                {
                    linkUrl = linkUrl.Replace("{SteamId}", game.GameId);
                }
                else
                {
                    return false;
                }
            }

            if (linkUrl.Contains("{GogId}"))
            {
                if (game.PluginId == Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e"))
                {
                    linkUrl = linkUrl.Replace("{GogId}", game.GameId);
                }
                else
                {
                    return false;
                }
            }

            if (linkUrl.Contains("{RomName}"))
            {
                if (game.IsInstalled && (game.Roms?.Any() ?? false))
                {
                    linkUrl = linkUrl.Replace("{RomName}", Path.GetFileNameWithoutExtension(game.Roms[0].Path));
                }
                else
                {
                    return false;
                }
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

        public override string GetBrowserSearchLink(string searchTerm) => BrowserSearchUrl.Replace("{GameName}", searchTerm.UrlEncode());

        public override string GetGamePath(Game game, string gameName = null) => CustomSettings.FormatGameName(game.Name);
    }
}