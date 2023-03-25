using KNARZhelper;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Metacritic.
    /// </summary>
    internal class LinkMetacritic : BaseClasses.Link
    {
        public override string LinkName { get; } = "Metacritic";
        public override string BaseUrl { get; } = "https://www.metacritic.com/game/";
        public override string SearchUrl { get; } = string.Empty;

        /// <summary>
        /// Dictionary with playnite platform ids and their equivalents in metacritic links
        /// </summary>
        private readonly IReadOnlyDictionary<string, string> _platforms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "nintendo_3ds", "3ds" },
            { "nintendo_64", "nintendo-64" },
            { "nintendo_ds", "ds" },
            { "nintendo_gameboyadvance", "gba" },
            { "nintendo_gamecube", "gamecube" },
            { "nintendo_switch", "switch" },
            { "nintendo_wii", "wii" },
            { "nintendo_wiiu", "wii-u" },
            { "pc_dos", "pc" },
            { "pc_linux", "pc" },
            { "pc_windows", "pc" },
            { "sega_dreamcast", "dreamcast" },
            { "sony_playstation", "playstation" },
            { "sony_playstation2", "playstation-2" },
            { "sony_playstation3", "playstation-3" },
            { "sony_playstation4", "playstation-4" },
            { "sony_playstation5", "playstation-5" },
            { "sony_psp", "psp" },
            { "sony_vita", "playstation-vita" },
            { "xbox", "xbox" },
            { "xbox360", "xbox-360" },
            { "xbox_one", "xbox-one" },
            { "xbox_series", "xbox-series-x" },
        };

        public override bool AddLink(Game game)
        {
            bool result = false;

            bool addPlatformName = game.Platforms.Count > 1;

            string linkName = LinkName;

            // Since Metacritic has an own link for every platform, we'll go through all of them and add one for each.
            foreach (Platform platform in game.Platforms.Where(x => x.SpecificationId != null))
            {
                if (addPlatformName)
                {
                    linkName = $"{LinkName} ({platform.Name})";
                }

                if (!LinkHelper.LinkExists(game, linkName))
                {
                    if (_platforms.ContainsKey(platform.SpecificationId))
                    {
                        LinkUrl = $"{BaseUrl}{_platforms[platform.SpecificationId]}/{GetGamePath(game)}";

                        if (CheckLink(LinkUrl))
                        {
                            result = LinkHelper.AddLink(game, linkName, LinkUrl, Plugin) || result;
                        }
                        else if (game.Name != game.Name.RemoveEditionSuffix())
                        {
                            LinkUrl = $"{BaseUrl}{_platforms[platform.SpecificationId]}/{GetGamePath(game, game.Name.RemoveEditionSuffix())}";

                            if (CheckLink(LinkUrl))
                            {
                                result = LinkHelper.AddLink(game, linkName, LinkUrl, Plugin) || result;
                            }
                        }
                    }
                }
            }

            return result;
        }

        // Metacritic Links need the game name in lowercase without special characters and hyphens instead of white spaces.
        public override string GetGamePath(Game game, string gameName = null)
            => (gameName ?? game.Name).RemoveSpecialChars()
                .CollapseWhitespaces()
                .Replace(" ", "-")
                .ToLower();

        public LinkMetacritic(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}