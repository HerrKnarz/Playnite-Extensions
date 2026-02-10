using Playnite.SDK.Models;
using PlayniteExtensions.Tests.Common;
using System.Collections.Generic;
using System.Linq;
using WikipediaMetadata.Models;
using WikipediaMetadata.Test.Fakes;
using Xunit;

namespace WikipediaMetadata.Test;

public class MetadataProviderTests
{
    private static List<string> GetTags(string gameName, string slug, string folderName)
    {
        var game = new Game(gameName);
        var settings = new PluginSettings();
        settings.PopulateTagSettings();
        var db = new FakePlayniteDatabase();
        var playniteApi = new FakePlayniteApi { Database = db };
        var downloader = new FakeWebClient(new()
        {
            { WikipediaApiUrl.GetArticleSearchUrl(gameName), $"data/{folderName}/search.json" },
            { WikipediaApiUrl.GetPageDataUrl(slug), $"data/{folderName}/article.json" },
            { WikipediaApiUrl.GetPagePropertiesUrl(slug), $"data/{folderName}/article-properties.json" },
        });

        db.Games.Add(game);

        var wikipediaApi = new WikipediaApi(downloader);
        var metadataProvider = new MetadataProvider(new(game, backgroundDownload: true), settings, playniteApi, wikipediaApi);
        var tags = metadataProvider.GetTags(new());
        var tagNames = tags.OfType<MetadataNameProperty>().Select(p => p.Name).ToList();
        return tagNames;
    }

    private static IEnumerable<string> GetCategoryNames(string gameName, string slug, string folderName)
    {
        var tags = GetTags(gameName, slug, folderName);
        return GetCategoryNames(tags);
    }

    private static IEnumerable<string> GetCategoryNames(IEnumerable<string> tags)
    {
        foreach (string tag in tags)
        {
            const string prefix = "[Category] ";
            if (tag.StartsWith(prefix))
                yield return tag.Substring(prefix.Length);
        }
    }

    [Fact]
    public void Doom3Categories()
    {
        var categories = GetCategoryNames("Doom 3", "Doom_3", "doom3").ToList();

        Assert.DoesNotContain("Activision games", categories);
        Assert.DoesNotContain("All Wikipedia articles written in American English", categories);
        Assert.DoesNotContain("Articles using Infobox video game using locally defined parameters", categories);
        Assert.DoesNotContain("Articles using Video game reviews template in multiple platform mode", categories);
        Assert.DoesNotContain("Articles using Video game reviews template in single platform mode", categories);
        Assert.DoesNotContain("Articles using Wikidata infoboxes with locally defined images", categories);
        Assert.DoesNotContain("Articles with short description", categories);
        Assert.DoesNotContain("CS1: unfit URL", categories);
        Assert.DoesNotContain("Doom (franchise) games", categories);
        Assert.DoesNotContain("First-person shooters", categories);
        Assert.DoesNotContain("Id Software games", categories);
        Assert.DoesNotContain("Id Tech 4 games", categories);
        Assert.DoesNotContain("Linux games", categories);
        Assert.DoesNotContain("MacOS games", categories);
        Assert.DoesNotContain("Multiplayer and single-player video games", categories);
        Assert.DoesNotContain("Multiplayer online games", categories);
        Assert.DoesNotContain("Nintendo Switch games", categories);
        Assert.DoesNotContain("PlayStation 3 games", categories);
        Assert.DoesNotContain("PlayStation 4 games", categories);
        Assert.DoesNotContain("Short description is different from Wikidata", categories);
        Assert.DoesNotContain("Use American English from May 2025", categories);
        Assert.DoesNotContain("Use mdy dates from May 2025", categories);
        Assert.DoesNotContain("Windows games", categories);
        Assert.DoesNotContain("Xbox 360 games", categories);
        Assert.DoesNotContain("Xbox One games", categories);
        Assert.DoesNotContain("Xbox games", categories);

        AssertContainsExclusively(categories,
                                  "2000s horror video games",
                                  "2004 video games",
                                  "Commercial video games with freely available source code",
                                  "Golden Joystick Award for Game of the Year winners",
                                  "Good articles",
                                  "Science fiction horror video games",
                                  "Science fiction video games",
                                  "Vicarious Visions games",
                                  "Video game reboots",
                                  "Video games about Satanism",
                                  "Video games about demons",
                                  "Video games developed in the United States",
                                  "Video games set in hell",
                                  "Video games set in the 22nd century",
                                  "Video games set on Mars",
                                  //Unwanted but probably unavoidable tags, due to them not being in the infobox:
                                  "Aspyr games",
                                  "Bethesda Softworks games",
                                  "Cooperative video games",
                                  "Panic Button (company) games",
                                  "PlayStation VR games",
                                  "Splash Damage games",
                                  "Xbox Cloud Gaming games");
    }

    [Fact]
    public void NiohCategories()
    {
        var categories = GetCategoryNames("Nioh", "Nioh", "nioh").ToList();

        Assert.DoesNotContain("Hack and slash role-playing games", categories);
        Assert.DoesNotContain("Multiplayer and single-player video games", categories);
        Assert.DoesNotContain("PlayStation 4 games", categories);
        Assert.DoesNotContain("PlayStation 4 Pro enhanced games", categories);
        Assert.DoesNotContain("PlayStation 5 games", categories);
        Assert.DoesNotContain("Sony Interactive Entertainment games", categories);
        Assert.DoesNotContain("Team Ninja games", categories);
        Assert.DoesNotContain("Koei Tecmo games", categories);
        Assert.DoesNotContain("Windows games", categories);

        AssertContainsExclusively(categories,
                                  "2017 video games",
                                  "Adaptations of works by Akira Kurosawa",
                                  "Cancelled PlayStation 3 games",
                                  "Video games about demons",
                                  "Video games about ninja",
                                  "Dark fantasy role-playing video games",
                                  "Soulslike video games",
                                  "Video games about samurai",
                                  "Fiction set in 17th-century Sengoku period",
                                  "Video games based on Japanese mythology",
                                  "Video games developed in Japan",
                                  "Video games set in feudal Japan",
                                  "Video games set in Japan",
                                  "Video games set in London",
                                  "Video games set in the 1600s",
                                  "Works about the Battle of Sekigahara");
    }

    private static void AssertContainsExclusively<T>(List<T> collection, params T[] expected)
    {
        foreach (var expectedItem in expected)
            Assert.Contains(expectedItem, collection);

        var unwantedItems = collection.Except(expected).ToList();
        Assert.Empty(unwantedItems);
    }
}
