using LinkUtilities.Helper;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to Metacritic.
    /// </summary>
    class LinkMetacritic : Link
    {
        public override string LinkName { get; } = "Metacritic";
        public override string BaseUrl { get; } = "https://www.metacritic.com/game/";
        public override string SearchUrl { get; } = string.Empty;

        /// <summary>
        /// Dictionary with possible platform names and their equivalents in metacritic links. Only needed for names that differ from
        /// the platform in the link. 
        /// </summary>
        public static IReadOnlyDictionary<string, string> Platforms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                                                                   {
                                                                        { "PC (Windows)", "pc" },
                                                                        { "PC (DOS)", "pc" },
                                                                        { "PC (Linux)", "pc" },
                                                                        { "N64", "nintendo-64" },
                                                                        { "Nintendo 3DS", "3ds" },
                                                                        { "Nintendo DS", "ds" },
                                                                        { "Nintendo Gamecube", "gamecube" },
                                                                        { "Nintendo Wii", "wii" },
                                                                        { "Nintendo Wii-U", "wii-u" },
                                                                        { "Nintendo Switch", "switch" },
                                                                        { "Playstation 1", "playstation" },
                                                                        { "PS1", "playstation" },
                                                                        { "PS One", "playstation" },
                                                                        { "PS2", "playstation-2" },
                                                                        { "PS3", "playstation-3" },
                                                                        { "PS4", "playstation-4" },
                                                                        { "PS5", "playstation-5" },
                                                                        { "PSP", "psp" },
                                                                        { "Playstation Portable", "psp" },
                                                                        { "PS Vita", "playstation-vita" },
                                                                        { "X360", "xbox-360" },
                                                                        { "XOne", "xbox-one" },
                                                                        { "XBSX", "xbox-series-x" },
                                                                        { "Sega Dreamcast", "dreamcast" }
                                                                   };
        public override bool AddLink(Game game)
        {
            bool result = false;

            string gameName = GetGamePath(game);

            bool addPlatformName = game.Platforms.Count > 1;

            string linkName = LinkName;

            // Since Metacritic has an own link for every platform, we'll go through all of them and add one for each.
            foreach (string platformName in game.Platforms.Select(x => x.Name))
            {
                if (addPlatformName)
                {
                    linkName = $"{LinkName} ({platformName})";
                }

                if (!LinkHelper.LinkExists(game, linkName))
                {
                    StringBuilder sb = new StringBuilder(platformName);

                    sb.Replace("Sony", "");
                    sb.Replace("Microsoft", "");
                    sb.Replace("Sega", "");

                    string platformNormalized = sb.ToString();

                    if (Platforms.ContainsKey(platformNormalized))
                    {
                        platformNormalized = Platforms[platformNormalized];
                    }

                    platformNormalized = platformNormalized.RemoveSpecialChars().CollapseWhitespaces().Replace(" ", "-").ToLower();

                    LinkUrl = $"{BaseUrl}{platformNormalized}/{gameName}";

                    if (CheckLink(LinkUrl))
                    {
                        result = LinkHelper.AddLink(game, linkName, LinkUrl, Plugin) || result;
                    }
                }
            }

            return result;
        }

        public override string GetGamePath(Game game)
        {
            // Metacritic Links need the game name in lowercase without special characters and hyphens instead of white spaces.
            return game.Name.RemoveSpecialChars().CollapseWhitespaces().Replace(" ", "-").ToLower();
        }

        public LinkMetacritic(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}