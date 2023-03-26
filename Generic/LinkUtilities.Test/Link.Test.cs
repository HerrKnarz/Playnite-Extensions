using LinkUtilities.Linker;
using LinkUtilities.Settings;
using Playnite.SDK.Models;
using Xunit;

namespace LinkUtilities.Test
{
    public class LinkTest
    {
        [Theory]
        [ClassData(typeof(LinkTestCases))]
        internal void TestGetGamePath(BaseClasses.Link linker, string gameName, string gamePath, string Url)
        {
            // Set the plugin to test mode
            _ = GlobalSettings.Instance(true);

            Game game = new Game
            {
                Name = gameName
            };

            Assert.Equal(gamePath, linker.GetGamePath(game));
        }
    }
}
