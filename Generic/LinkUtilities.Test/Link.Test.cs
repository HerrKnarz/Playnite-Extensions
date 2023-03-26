using LinkUtilities.Settings;
using Playnite.SDK.Models;
using System.Linq;
using Xunit;

namespace LinkUtilities.Test
{
    public class LinkTest
    {
        [Theory]
        [ClassData(typeof(TestCases))]
        internal void TestGetGamePath(TestCase testCase)
        {
            // Set the plugin to test mode
            _ = GlobalSettings.Instance(true);

            Game game = new Game
            {
                Name = testCase.GameName
            };

            Assert.Equal(testCase.GamePath, testCase.Link.GetGamePath(game));
        }

        [Theory]
        [ClassData(typeof(TestCases))]
        internal void TestAddLink(TestCase testCase)
        {
            // Set the plugin to test mode
            _ = GlobalSettings.Instance(true);

            Game game = new Game
            {
                Name = testCase.GameName
            };

            if (testCase.GamePath.Any())
            {
                Assert.True(testCase.Link.AddLink(game));
            }

            Assert.Equal(testCase.Url, game.Links?.FirstOrDefault()?.Url ?? "not found!");
        }

        [Theory]
        [ClassData(typeof(TestCases))]
        internal void TestAddSearchedLink(TestCase testCase)
        {
            // Set the plugin to test mode
            _ = GlobalSettings.Instance(true);

            Game game = new Game
            {
                Name = testCase.GameName
            };

            Assert.True(testCase.Link.AddSearchedLink(game));

            Assert.Equal(testCase.SearchedUrl, game.Links?.FirstOrDefault()?.Url ?? "not found!");
        }
    }
}
