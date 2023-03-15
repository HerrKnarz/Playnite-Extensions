using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KNARZhelper
{
    /// <summary>
    /// Class to match external platform names to the existing platforms in Playnite. Shamelessly partly copied from Jeshibu:
    /// https://github.com/Jeshibu/PlayniteExtensions/blob/590a4a10d2223b12ecc742d908707ab34841ea65/source/PlayniteExtensions.Common/PlatformUtility.cs
    /// </summary>
    public class PlatformHelper
    {
        private readonly Dictionary<string, string[]> platformSpecNameByNormalName;
        private readonly Regex TrimCompanyName = new Regex(@"^(atari|bandai|coleco|commodore|mattel|nec|nintendo|sega|sinclair|snk|sony|microsoft)?\s+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private readonly Regex TrimInput = new Regex(@"^(pal|jpn?|usa?|ntsc)\s+|[™®©]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public PlatformHelper(IPlayniteAPI api)
        {
            platformSpecNameByNormalName = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase);

            List<Platform> platforms = api.Database.Platforms.Where(p => p.SpecificationId != null).ToList();
            foreach (Platform platform in platforms)
            {
                platformSpecNameByNormalName.Add(platform.Name, new[] { platform.SpecificationId });

                string nameWithoutCompany = TrimCompanyName.Replace(platform.Name, string.Empty);

                if (!platformSpecNameByNormalName.ContainsKey(nameWithoutCompany))
                {
                    platformSpecNameByNormalName.Add(nameWithoutCompany, new[] { platform.SpecificationId });
                }
            }
            TryAddPlatformByName(platformSpecNameByNormalName, "3DO", "3do");
            TryAddPlatformByName(platformSpecNameByNormalName, new[] { "Microsoft Windows", "Windows", "PC", "PC CD-ROM", "PC DVD", "PC DVD-ROM", "Windows 95" }, new[] { "pc_windows" });
            TryAddPlatformByName(platformSpecNameByNormalName, new[] { "DOS", "MS-DOS" }, new[] { "pc_dos" });
            TryAddPlatformByName(platformSpecNameByNormalName, "Linux", "pc_linux");
            TryAddPlatformByName(platformSpecNameByNormalName, new[] { "Mac", "OSX", "OS X", "MacOS", "Mac OS", "Mac OS X" }, new[] { "macintosh" });
            TryAddPlatformByName(platformSpecNameByNormalName, new[] { "Microsoft Xbox Series X", "Microsoft Xbox Series S", "Xbox Series X", "Xbox Series S", "Microsoft Xbox Series X/S", "Microsoft Xbox Series S/X", "Xbox Series X/S", "Xbox Series S/X", "Xbox Series X|S", }, new[] { "xbox_series" });
            TryAddPlatformByName(platformSpecNameByNormalName, new[] { "PS", "PS1", "PSX" }, new[] { "sony_playstation" });
            TryAddPlatformByName(platformSpecNameByNormalName, "PS2", "sony_playstation2");
            TryAddPlatformByName(platformSpecNameByNormalName, "PS3", "sony_playstation3");
            TryAddPlatformByName(platformSpecNameByNormalName, "PS4", "sony_playstation4");
            TryAddPlatformByName(platformSpecNameByNormalName, "PS5", "sony_playstation5");
            TryAddPlatformByName(platformSpecNameByNormalName, "PSP", "sony_psp");
            TryAddPlatformByName(platformSpecNameByNormalName, "Vita", "sony_vita");
            TryAddPlatformByName(platformSpecNameByNormalName, "PS4/5", new[] { "sony_playstation4", "sony_playstation5" });
            TryAddPlatformByName(platformSpecNameByNormalName, "Playstation 4/5", new[] { "sony_playstation4", "sony_playstation5" });
            TryAddPlatformByName(platformSpecNameByNormalName, new[] { "Sega Mega Drive", "Mega Drive", "Mega Drive/Genesis" }, new[] { "sega_genesis" });
            TryAddPlatformByName(platformSpecNameByNormalName, new[] { "Super NES", "Super Nintendo Entertainment System" }, new[] { "nintendo_super_nes" });
            //TryAddPlatformByName(platformSpecNameByNormalName, new[] { "SNK Neo Geo MVS", "Neo Geo MVS" }, new[] { "snk_neo_geo" });
        }
        private bool TryAddPlatformByName(Dictionary<string, string[]> dict, string platformName, params string[] platformSpecNames)
        {
            if (dict.ContainsKey(platformName))
            {
                return false;
            }

            dict.Add(platformName, platformSpecNames);
            return true;
        }

        private bool TryAddPlatformByName(Dictionary<string, string[]> dict, string[] platformNames, params string[] platformSpecNames)
        {
            bool success = true;
            foreach (string platformName in platformNames)
            {
                success &= TryAddPlatformByName(dict, platformName, platformSpecNames);
            }
            return success;
        }
        /// <summary>
        /// returns all platforms created in Playnite that fit the platform name 
        /// </summary>
        /// <param name="platformName">Name of the platform</param>
        /// <returns>List of platforms</returns>
        public IEnumerable<MetadataProperty> GetPlatforms(string platformName)
        {
            return GetPlatforms(platformName, strict: false);
        }

        /// <summary>
        /// returns all platforms created in Playnite that fit the platform name 
        /// </summary>
        /// <param name="platformName">Name of the platform</param>
        /// <param name="strict">If true, only matches will be returned. If false the function also returns all platforms,
        /// that aren't found at all.</param>
        /// <returns>List of platforms</returns>
        public IEnumerable<MetadataProperty> GetPlatforms(string platformName, bool strict)
        {
            if (string.IsNullOrWhiteSpace(platformName))
            {
                return new List<MetadataProperty>();
            }

            string sanitizedPlatformName = TrimInput.Replace(platformName, string.Empty);

            if (platformSpecNameByNormalName.TryGetValue(sanitizedPlatformName, out string[] specIds))
            {
                return specIds.Select(s => new MetadataSpecProperty(s)).ToList<MetadataProperty>();
            }

            if (strict)
            {
                return new List<MetadataProperty>();
            }
            else
            {
                return new List<MetadataProperty> { new MetadataNameProperty(sanitizedPlatformName) };
            }
        }
    }
}
