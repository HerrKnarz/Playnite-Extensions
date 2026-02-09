using WikipediaMetadata.Models;
using WikipediaMetadata.Test.Fakes;
using Xunit;

namespace WikipediaMetadata.Test;

public class GameFinderTest
{
    private GameFinder Setup(string internalSearchQuery, string fileName)
    {
        var settings = new PluginSettings();
        var webclient = new FakeWebClient(WikipediaApiUrl.GetArticleSearchUrl(internalSearchQuery), $"data/GameFinder/{fileName}.json");
        var api = new WikipediaApi(webclient);

        return new GameFinder(api, settings.AdvancedSearchResultSorting);
    }

    [Fact]
    public void VampireSurvivors()
    {
        const string gameName = "Vampire Survivors";
        var gameFinder = Setup(gameName, "vampire-survivors");
        Assert.Equal("Vampire_Survivors", gameFinder.FindGame(gameName)?.Key);
    }

    [Fact]
    public void Doom()
    {
        const string gameName = "Doom";
        var gameFinder = Setup(gameName, "doom");
        Assert.Null(gameFinder.FindGame(gameName)?.Key);
    }

    [Fact]
    public void PlanescapeTorment()
    {
        var gameFinder = Setup("Planescape:  Torment","planescape-torment");
        Assert.Equal("Planescape:_Torment", gameFinder.FindGame("Planescape:  Torment: Enhanced Edition")?.Key);
    }

    [Fact]
    public void MassEffect()
    {
        var gameFinder = Setup("Mass Effect","mass-effect");
        Assert.Equal("Mass_Effect_Legendary_Edition", gameFinder.FindGame("Mass Effect Legendary Edition")?.Key);
    }
}
