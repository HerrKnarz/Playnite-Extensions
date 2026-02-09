using Playnite.SDK.Models;
using PlayniteExtensions.Tests.Common;
using System.Linq;
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
        var downloader = new FakeWebClient(new()
        {
            { WikipediaApiUrl.GetArticleSearchUrl(game.Name), "data/doom3/search.json" },
            { WikipediaApiUrl.GetPageDataUrl("Doom_3"), "data/doom3/article.json" },
            { WikipediaApiUrl.GetPagePropertiesUrl("Doom_3"), "data/doom3/article-images.json" },
        });

        db.Games.Add(game);

        var wikipediaApi = new WikipediaApi(downloader);
        var metadataProvider = new MetadataProvider(new(game, backgroundDownload: true), settings, playniteApi, wikipediaApi);
        var tags = metadataProvider.GetTags(new());
        var tagNames = tags.OfType<MetadataNameProperty>().Select(p => p.Name).ToList();

        Assert.Contains("[Category] 2000s horror video games", tagNames);
        Assert.Contains("[Category] 2004 video games", tagNames);
        Assert.Contains("[Category] Commercial video games with freely available source code", tagNames);
        Assert.Contains("[Category] Golden Joystick Award for Game of the Year winners", tagNames);
        Assert.Contains("[Category] Good articles", tagNames);
        Assert.Contains("[Category] Science fiction horror video games", tagNames);
        Assert.Contains("[Category] Science fiction video games", tagNames);
        Assert.Contains("[Category] Vicarious Visions games", tagNames);
        Assert.Contains("[Category] Video game reboots", tagNames);
        Assert.Contains("[Category] Video games about Satanism", tagNames);
        Assert.Contains("[Category] Video games about demons", tagNames);
        Assert.Contains("[Category] Video games developed in the United States", tagNames);
        Assert.Contains("[Category] Video games set in hell", tagNames);
        Assert.Contains("[Category] Video games set in the 22nd century", tagNames);
        Assert.Contains("[Category] Video games set on Mars", tagNames);

        Assert.DoesNotContain("[Category] Activision games", tagNames);
        Assert.DoesNotContain("[Category] All Wikipedia articles written in American English", tagNames);
        Assert.DoesNotContain("[Category] Articles using Infobox video game using locally defined parameters", tagNames);
        Assert.DoesNotContain("[Category] Articles using Video game reviews template in multiple platform mode", tagNames);
        Assert.DoesNotContain("[Category] Articles using Video game reviews template in single platform mode", tagNames);
        Assert.DoesNotContain("[Category] Articles using Wikidata infoboxes with locally defined images", tagNames);
        Assert.DoesNotContain("[Category] Articles with short description", tagNames);
        Assert.DoesNotContain("[Category] CS1: unfit URL", tagNames);
        Assert.DoesNotContain("[Category] Doom (franchise) games", tagNames);
        Assert.DoesNotContain("[Category] First-person shooters", tagNames);
        Assert.DoesNotContain("[Category] Id Software games", tagNames);
        Assert.DoesNotContain("[Category] Id Tech 4 games", tagNames);
        Assert.DoesNotContain("[Category] Linux games", tagNames);
        Assert.DoesNotContain("[Category] MacOS games", tagNames);
        Assert.DoesNotContain("[Category] Multiplayer and single-player video games", tagNames);
        Assert.DoesNotContain("[Category] Multiplayer online games", tagNames);
        Assert.DoesNotContain("[Category] Nintendo Switch games", tagNames);
        Assert.DoesNotContain("[Category] PlayStation 3 games", tagNames);
        Assert.DoesNotContain("[Category] PlayStation 4 games", tagNames);
        Assert.DoesNotContain("[Category] Short description is different from Wikidata", tagNames);
        Assert.DoesNotContain("[Category] Use American English from May 2025", tagNames);
        Assert.DoesNotContain("[Category] Use mdy dates from May 2025", tagNames);
        Assert.DoesNotContain("[Category] Windows games", tagNames);
        Assert.DoesNotContain("[Category] Xbox 360 games", tagNames);
        Assert.DoesNotContain("[Category] Xbox One games", tagNames);
        Assert.DoesNotContain("[Category] Xbox games", tagNames);


        //Unwanted but probably unavoidable tags, due to them not being in the infobox:
        Assert.Contains("[Category] Aspyr games", tagNames);
        Assert.Contains("[Category] Bethesda Softworks games", tagNames);
        Assert.Contains("[Category] Cooperative video games", tagNames);
        Assert.Contains("[Category] Panic Button (company) games", tagNames);
        Assert.Contains("[Category] PlayStation VR games", tagNames);
        Assert.Contains("[Category] Splash Damage games", tagNames);
        Assert.Contains("[Category] Xbox Cloud Gaming games", tagNames);
    }
}
