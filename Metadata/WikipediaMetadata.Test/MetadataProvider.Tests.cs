using Playnite.SDK.Models;
using PlayniteExtensions.Tests.Common;
using System.Linq;
using WikipediaMetadata.Categories;
using WikipediaMetadata.Models;
using WikipediaMetadata.Test.Fakes;
using Xunit;

namespace WikipediaMetadata.Test;

public class MetadataProviderTests
{
    [Fact]
    public void Doom3Categories()
    {
        var game = new Game("Doom 3");
        var settings = new PluginSettings();
        settings.PopulateTagSettings();
        var db = new FakePlayniteDatabase();
        var playniteApi = new FakePlayniteApi { Database = db };
        var downloader = new FakeWebDownloader(new()
        {
            { "https://en.wikipedia.org/w/rest.php/v1/search/page?q=Doom+3&limit=100", "Resources/doom3/search.json" },
            { "https://en.wikipedia.org/w/rest.php/v1/page/Doom_3", "Resources/doom3/article.json" },
            { "https://en.wikipedia.org/w/api.php?format=json&action=query&titles=Doom_3&prop=categories%7Credirects&cllimit=max&rdlimit=max&redirects", "Resources/doom3/article-categories.json" },
            { "https://en.wikipedia.org/w/api.php?action=query&format=json&formatversion=2&prop=pageimages|pageterms&piprop=original&pilicense=any&titles=Doom_3", "Resources/doom3/article-images.json" },
        });
        WikipediaApiCaller.WebClientOverride = downloader;

        db.Games.Add(game);

        var wikipediaApi = new WikipediaApi(downloader, new(10, 50));
        var metadataProvider = new MetadataProvider(new(game, backgroundDownload: true), settings, playniteApi, wikipediaApi);
        var tags = metadataProvider.GetTags(new());
        var tagNames = tags.OfType<MetadataNameProperty>().Select(p => p.Name).ToList();

        Assert.Contains("2000s horror video games", tagNames);
        Assert.Contains("2004 video games", tagNames);
        Assert.Contains("Commercial video games with freely available source code", tagNames);
        Assert.Contains("Golden Joystick Award for Game of the Year winners", tagNames);
        Assert.Contains("Good articles", tagNames);
        Assert.Contains("Science fiction horror video games", tagNames);
        Assert.Contains("Science fiction video games", tagNames);
        Assert.Contains("Vicarious Visions games", tagNames);
        Assert.Contains("Video game reboots", tagNames);
        Assert.Contains("Video games about Satanism", tagNames);
        Assert.Contains("Video games about demons", tagNames);
        Assert.Contains("Video games developed in the United States", tagNames);
        Assert.Contains("Video games set in hell", tagNames);
        Assert.Contains("Video games set in the 22nd century", tagNames);
        Assert.Contains("Video games set on Mars", tagNames);

        Assert.DoesNotContain("Activision games", tagNames);
        Assert.DoesNotContain("All Wikipedia articles written in American English", tagNames);
        Assert.DoesNotContain("Articles using Infobox video game using locally defined parameters", tagNames);
        Assert.DoesNotContain("Articles using Video game reviews template in multiple platform mode", tagNames);
        Assert.DoesNotContain("Articles using Video game reviews template in single platform mode", tagNames);
        Assert.DoesNotContain("Articles using Wikidata infoboxes with locally defined images", tagNames);
        Assert.DoesNotContain("Articles with short description", tagNames);
        Assert.DoesNotContain("CS1: unfit URL", tagNames);
        Assert.DoesNotContain("Doom (franchise) games", tagNames);
        Assert.DoesNotContain("First-person shooters", tagNames);
        Assert.DoesNotContain("Id Software games", tagNames);
        Assert.DoesNotContain("Id Tech 4 games", tagNames);
        Assert.DoesNotContain("Linux games", tagNames);
        Assert.DoesNotContain("Multiplayer and single-player video games", tagNames);
        Assert.DoesNotContain("Multiplayer online games", tagNames);
        Assert.DoesNotContain("Nintendo Switch games", tagNames);
        Assert.DoesNotContain("PlayStation 3 games", tagNames);
        Assert.DoesNotContain("PlayStation 4 games", tagNames);
        Assert.DoesNotContain("Short description is different from Wikidata", tagNames);
        Assert.DoesNotContain("Use American English from May 2025", tagNames);
        Assert.DoesNotContain("Use mdy dates from May 2025", tagNames);
        Assert.DoesNotContain("Windows games", tagNames);
        Assert.DoesNotContain("Xbox 360 games", tagNames);
        Assert.DoesNotContain("Xbox One games", tagNames);

        //Unwanted but probably unavoidable tags, due to them not being in the infobox:
        Assert.Contains("Aspyr games", tagNames);
        Assert.Contains("Bethesda Softworks games", tagNames);
        Assert.Contains("Cooperative video games", tagNames);
        Assert.Contains("MacOS games", tagNames); // infobox has Mac OS X, not MacOS
        Assert.Contains("Panic Button (company) games", tagNames);
        Assert.Contains("PlayStation VR games", tagNames);
        Assert.Contains("Splash Damage games", tagNames);
        Assert.Contains("Xbox Cloud Gaming games", tagNames);
        Assert.Contains("Xbox games", tagNames); // infobox links to "Xbox (console)"

    }
}
