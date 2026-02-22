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
        var api = Api(gameName, "doom");
        var searchResults = api.GetSearchResults(gameName);
        Assert.Null(Finder().FindGame(gameName, searchResults)?.Key);
    }

    [Fact]
    public void MassEffect()
    {
        var api = Api("Mass Effect", "mass-effect");
        var searchResults = api.GetSearchResults("Mass Effect Legendary Edition");
        Assert.Equal("Mass_Effect_Legendary_Edition", Finder().FindGame("Mass Effect Legendary Edition", searchResults)?.Key);
    }

    [Fact]
    public void PlanescapeTorment()
    {
        var api = Api("Planescape:  Torment", "planescape-torment");
        var searchResults = api.GetSearchResults("Planescape:  Torment");
        Assert.Equal("Planescape:_Torment", Finder().FindGame("Planescape:  Torment: Enhanced Edition", searchResults)?.Key);
    }

    [Fact]
    public void VampireSurvivors()
    {
        const string gameName = "Vampire Survivors";
        var api = Api(gameName, "vampire-survivors");
        var searchResults = api.GetSearchResults(gameName);
        Assert.Equal("Vampire_Survivors", Finder().FindGame(gameName, searchResults)?.Key);
    }

    private WikipediaApi Api(string internalSearchQuery, string fileName)
    {
        var webclient = new FakeWebClient(WikipediaApiUrl.GetArticleSearchUrl(internalSearchQuery), $"data/GameFinder/{fileName}.json");
        return new WikipediaApi(webclient);
    }

    private GameFinder Finder()
    {
        var settings = new PluginSettings();

        return new GameFinder(settings.AdvancedSearchResultSorting);
    }
}
