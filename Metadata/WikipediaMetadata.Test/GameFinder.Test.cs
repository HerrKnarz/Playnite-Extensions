using WikipediaMetadata.Models;
using WikipediaMetadata.Test.Fakes;
using Xunit;

namespace WikipediaMetadata.Test;

public class GameFinderTest
{
    [Fact]
    public void Doom()
    {
        const string gameName = "Doom";
        Assert.Null(Finder(gameName, "doom").FindGame(gameName)?.Key);
    }

    [Fact]
    public void MassEffect() => Assert.Equal("Mass_Effect_Legendary_Edition", Finder("Mass Effect", "mass-effect").FindGame("Mass Effect Legendary Edition")?.Key);

    [Fact]
    public void PlanescapeTorment() => Assert.Equal("Planescape:_Torment", Finder("Planescape:  Torment", "planescape-torment").FindGame("Planescape:  Torment: Enhanced Edition")?.Key);

    [Fact]
    public void VampireSurvivors()
    {
        const string gameName = "Vampire Survivors";
        Assert.Equal("Vampire_Survivors", Finder("Vampire Survivors", "vampire-survivors").FindGame(gameName)?.Key);
    }

    private GameFinder Finder(string internalSearchQuery, string fileName)
    {
        var httpclient = new FakeWebClient(WikipediaApiUrl.GetArticleSearchUrl(internalSearchQuery), $"data/GameFinder/{fileName}.json");
        var settings = new PluginSettings();

        return new GameFinder(httpclient, settings.AdvancedSearchResultSorting);
    }
}
