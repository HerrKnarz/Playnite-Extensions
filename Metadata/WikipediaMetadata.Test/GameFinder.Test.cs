using WikipediaMetadata.Models;
using Xunit;

namespace WikipediaMetadata.Test
{
    public class GameFinderTest
    {
        [Theory]
        [InlineData("Vampire Survivors", "Vampire_Survivors")]
        [InlineData("Doom", "")]
        [InlineData("Planescape:  Torment: Enhanced Edition", "Planescape:_Torment")]
        [InlineData("Mass Effect Legendary Edition", "Mass_Effect_Legendary_Edition")]
        public void TestFindGame(string stringToCheck, string stringResult)
        {
            PluginSettings settings = new PluginSettings();

            GameFinder gameFinder = new GameFinder(settings);

            Assert.Equal(stringResult, gameFinder.FindGame(stringToCheck)?.Key ?? string.Empty);
        }
    }
}