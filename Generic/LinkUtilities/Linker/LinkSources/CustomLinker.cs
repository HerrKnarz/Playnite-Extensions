using KNARZhelper;
using LinkUtilities.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LinkUtilities.Linker
{
    public class CustomLinker : BaseClasses.Linker
    {
        public CustomLinker(CustomLinkProfile customSettings)
        {
            CustomSettings = customSettings;

            Settings.IsCustomSource = true;
        }

        public CustomLinkProfile CustomSettings { get; }

        public override string LinkName => CustomSettings?.Name ?? string.Empty;

        public override string BaseUrl => CustomSettings?.GameUrl ?? string.Empty;
        public override string BrowserSearchUrl => CustomSettings?.BrowserSearchUrl ?? string.Empty;
        public override bool AllowRedirects => CustomSettings?.AllowRedirects ?? true;
        public override bool ReturnsSameUrl => CustomSettings?.ReturnsSameUrl ?? false;
        public override bool NeedsToBeChecked => CustomSettings?.NeedsToBeChecked ?? true;

        public override string GetGamePath(Game game, string gameName = null)
        {
            gameName = game.Name;


            if (CustomSettings.RemoveEditionSuffix)
            {
                gameName = gameName.RemoveEditionSuffix();
            }

            if (CustomSettings.RemoveHyphens)
            {
                gameName = gameName.Replace("-", "");
            }

            if (CustomSettings.UnderscoresToWhitespaces)
            {
                gameName = gameName.Replace("_", " ");
            }

            if (CustomSettings.RemoveSpecialChars)
            {
                gameName = gameName.RemoveSpecialChars();
            }

            if (CustomSettings.RemoveDiacritics)
            {
                gameName = gameName.RemoveDiacritics();
            }

            if (CustomSettings.ToTitleCase)
            {
                gameName = gameName.ToTitleCase();
            }

            if (CustomSettings.ToLower)
            {
                gameName = gameName.ToLower();
            }

            gameName = CustomSettings.RemoveWhitespaces ? gameName.Replace(" ", "") : gameName.CollapseWhitespaces();

            if (CustomSettings.WhitespacesToHyphens)
            {
                gameName = gameName.Replace(" ", "-");
            }

            if (CustomSettings.WhitespacesToUnderscores)
            {
                gameName = gameName.Replace(" ", "_");
            }

            if (CustomSettings.EscapeDataString)
            {
                gameName = gameName.EscapeDataString();
            }

            if (CustomSettings.UrlEncode)
            {
                gameName = gameName.UrlEncode();
            }

            return gameName;
        }

        public override bool FindLinks(Game game, out List<Link> links)
        {
            LinkUrl = string.Empty;
            links = new List<Link>();

            if (LinkHelper.LinkExists(game, LinkName))
            {
                return false;
            }

            string linkUrl = BaseUrl;

            if (linkUrl.Contains("{GameName}"))
            {
                string gameName = GetGamePath(game);

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
    }
}