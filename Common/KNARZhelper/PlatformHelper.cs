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
        private readonly Dictionary<string, string[]> _platformSpecNameByNormalName;
        private readonly Regex _trimCompanyName = new Regex(@"^(atari|bandai|coleco|commodore|mattel|nec|nintendo|sega|sinclair|snk|sony|microsoft)?\s+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private readonly Regex _trimInput = new Regex(@"^(pal|jpn?|usa?|ntsc)\s+|[™®©]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public PlatformHelper(IEnumerable<Platform> platforms)
        {
            _platformSpecNameByNormalName = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase);

            foreach (Platform platform in platforms.Where(p => p.SpecificationId != null))
            {
                _platformSpecNameByNormalName.Add(platform.Name, new[] { platform.SpecificationId });

                string nameWithoutCompany = _trimCompanyName.Replace(platform.Name, string.Empty);

                if (!_platformSpecNameByNormalName.ContainsKey(nameWithoutCompany))
                {
                    _platformSpecNameByNormalName.Add(nameWithoutCompany, new[] { platform.SpecificationId });
                }
            }

            TryAddPlatformByName(_platformSpecNameByNormalName, "3DO", "3do");
            TryAddPlatformByName(_platformSpecNameByNormalName,
                new[] { "Microsoft Windows", "Windows", "PC", "PC CD-ROM", "PC DVD", "PC DVD-ROM", "Windows 95" }, "pc_windows");
            TryAddPlatformByName(_platformSpecNameByNormalName, new[] { "DOS", "MS-DOS" }, "pc_dos");
            TryAddPlatformByName(_platformSpecNameByNormalName, "Linux", "pc_linux");
            TryAddPlatformByName(_platformSpecNameByNormalName, new[] { "Mac", "OSX", "OS X", "MacOS", "Mac OS", "Mac OS X" }, "macintosh");
            TryAddPlatformByName(_platformSpecNameByNormalName,
                new[]
                {
                    "Microsoft Xbox Series X", "Microsoft Xbox Series S", "Xbox Series X", "Xbox Series S",
                    "Microsoft Xbox Series X/S", "Microsoft Xbox Series S/X", "Xbox Series X/S", "Xbox Series S/X",
                    "Xbox Series X|S",
                }, "xbox_series");
            TryAddPlatformByName(_platformSpecNameByNormalName, new[] { "PS", "PS1", "PSX" }, "sony_playstation");
            TryAddPlatformByName(_platformSpecNameByNormalName, "PS2", "sony_playstation2");
            TryAddPlatformByName(_platformSpecNameByNormalName, "PS3", "sony_playstation3");
            TryAddPlatformByName(_platformSpecNameByNormalName, "PS4", "sony_playstation4");
            TryAddPlatformByName(_platformSpecNameByNormalName, "PS5", "sony_playstation5");
            TryAddPlatformByName(_platformSpecNameByNormalName, "PSP", "sony_psp");
            TryAddPlatformByName(_platformSpecNameByNormalName, "Vita", "sony_vita");
            TryAddPlatformByName(_platformSpecNameByNormalName, "PS4/5", "sony_playstation4", "sony_playstation5");
            TryAddPlatformByName(_platformSpecNameByNormalName, "Playstation 4/5", "sony_playstation4", "sony_playstation5");
            TryAddPlatformByName(_platformSpecNameByNormalName, new[] { "Sega Mega Drive", "Mega Drive", "Mega Drive/Genesis" }, "sega_genesis");
            TryAddPlatformByName(_platformSpecNameByNormalName, new[] { "Super NES", "Super Nintendo Entertainment System" }, "nintendo_super_nes");
            TryAddPlatformByName(_platformSpecNameByNormalName, new[] { "SNK Neo Geo MVS", "Neo Geo MVS", "SNK Neo Geo AES", "Neo Geo AES" }, "snk_neogeo_aes");
        }
        private static bool TryAddPlatformByName(IDictionary<string, string[]> dict, string platformName, params string[] platformSpecNames)
        {
            if (dict.ContainsKey(platformName))
            {
                return false;
            }

            dict.Add(platformName, platformSpecNames);
            return true;
        }

        private static bool TryAddPlatformByName(IDictionary<string, string[]> dict, IEnumerable<string> platformNames, params string[] platformSpecNames)
            => platformNames.Aggregate(true, (current, platformName) => current & TryAddPlatformByName(dict, platformName, platformSpecNames));

        /// <summary>
        /// returns all platforms created in Playnite that fit the platform name
        /// </summary>
        /// <param name="platformName">Name of the platform</param>
        /// <returns>List of platforms</returns>
        public IEnumerable<MetadataProperty> GetPlatforms(string platformName) => GetPlatforms(platformName, strict: false);

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

            string sanitizedPlatformName = _trimInput.Replace(platformName, string.Empty);

            return _platformSpecNameByNormalName.TryGetValue(sanitizedPlatformName, out string[] specIds)
                ? specIds.Select(s => new MetadataSpecProperty(s)).ToList<MetadataProperty>()
                : strict
                    ? new List<MetadataProperty>()
                    : new List<MetadataProperty> { new MetadataNameProperty(sanitizedPlatformName) };
        }
    }
}
