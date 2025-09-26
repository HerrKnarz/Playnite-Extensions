using LinkUtilities.Helper;
using Xunit;

namespace LinkUtilities.Test
{
    public class SteamHelperTests
    {
        [Theory]
        [InlineData("https://store.steampowered.com/app/275850", "275850")]
        [InlineData("https://steamcommunity.com/app/275850/guides/", "275850")]
        [InlineData("steam://openurl/https://store.steampowered.com/app/275850", "275850")]
        [InlineData("steam://openurl/https://steamcommunity.com/app/275850/guides/", "275850")]
        public void TestGetSteamIdFromUrl(string url, string steamId)
        {
            var result = SteamHelper.GetSteamIdFromUrl(url);

            Assert.Equal(steamId, result);
        }

        [Theory]
        [InlineData("https://store.steampowered.com/app/275850", true, "steam://openurl/https://store.steampowered.com/app/275850")]
        [InlineData("steam://openurl/https://store.steampowered.com/app/275850", false, "https://store.steampowered.com/app/275850")]
        public void TestChangeSteamLink(string url, bool toStoreLink, string expected)
        {
            var result = SteamHelper.ChangeSteamLink(url, toStoreLink);

            Assert.Equal(expected, result);
        }
    }
}
