using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace KNARZhelper.Test
{
    public class PlatformHelperTest
    {
        [Theory]
        [InlineData("Sega Mega Drive", "sega_genesis")]
        [InlineData("Genesis", "sega_genesis")]
        [InlineData("Super NES", "nintendo_super_nes")]
        [InlineData("Android", "Android")]
        [InlineData("Playstation 4/5", "sony_playstation4,sony_playstation5")]
        [InlineData("Neo Geo AES", "snk_neogeo_aes")]
        [InlineData("Neo Geo MVS", "snk_neogeo_aes")]
        public void TestGetPlatforms(string platformToFind, string platformResult)
        {
            List<Platform> platforms = new List<Platform>
            {
                new Platform()
                {
                    Name = "SNK Neo Geo",
                    Id = new Guid("dabf46e5-bd45-409a-bac2-16c158f9cad2"),
                    SpecificationId = "snk_neo_geo_aes"
                },
                new Platform()
                {
                    Name = "Sony PlayStation 4",
                    Id = new Guid("aabf46e5-bd45-409a-bac2-16c158f9cad2"),
                    SpecificationId = "sony_playstation4"
                },
                new Platform()
                {
                    Name = "Sony PlayStation 5",
                    Id = new Guid("babf46e5-bd45-409a-bac2-16c158f9cad2"),
                    SpecificationId = "sony_playstation5"
                },
                new Platform()
                {
                    Name = "Sega Genesis",
                    Id = new Guid("adf5bbff-8fbe-438c-99fb-7a4dabe4d24f"),
                    SpecificationId = "sega_genesis"
                },
                new Platform()
                {
                    Name = "Super Nintendo Entertainment System",
                    Id = new Guid("2739af88-ff78-4b54-a90c-41410f541612"),
                    SpecificationId = "nintendo_super_nes"
                },
                new Platform()
                {
                    Name = "Kaneko Super Nova System",
                    Id = new Guid("1ced4513-9326-4701-a814-7d3c93bcc5a5")
                },
            };


            PlatformHelper platformHelper = new PlatformHelper(platforms);

            Assert.Equal(platformResult, string.Join(",", platformHelper.GetPlatforms(platformToFind)));
        }
    }
}
