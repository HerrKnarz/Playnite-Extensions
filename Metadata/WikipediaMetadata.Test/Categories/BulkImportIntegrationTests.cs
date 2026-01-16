using Playnite.SDK.Models;
using PlayniteExtensions.Common;
using PlayniteExtensions.Tests.Common;
using System.Linq;
using WikipediaMetadata.Categories;
using WikipediaMetadata.Categories.Models;
using WikipediaMetadata.Test.Categories.Fakes;
using Xunit;

namespace WikipediaMetadata.Test.Categories;

public class BulkImportIntegrationTests
{
    private WikipediaCategoryBulkImport Setup(string search, out FakePlayniteDatabase db, out FakeWebDownloader downloader, out WikipediaApi api)
    {
        db = new FakePlayniteDatabase();
        var ui = new FakeWikipediaBulkImportUserInterface(search, _ => true, _ => true);
        downloader = new FakeWebDownloader();
        api = new WikipediaApi(downloader, new(10, 47), "en");
        return new WikipediaCategoryBulkImport(db, ui, new(api), new PlatformUtility(), 1);
    }

    [Fact]
    public void SixteenthCenturyImports()
    {
        var search = "video games set in the 16th century";
        var bi = Setup(search, out var db, out var downloader, out var api);

        downloader.FilesByUrl[api.GetSearchUrl(search, WikipediaNamespace.Category)] = "Resources/16th-century/search.json";
        void AddCategoryUrlFile(string categoryName) => downloader.FilesByUrl.Add(api.GetCategoryMembersUrl($"Category:{categoryName}"), $"Resources/16th-century/{categoryName}.json");
        AddCategoryUrlFile("Inuyasha games");
        AddCategoryUrlFile("Video games set in the 16th century");
        AddCategoryUrlFile("Video games set in the 1500s");
        AddCategoryUrlFile("Video games set in the 1510s");
        AddCategoryUrlFile("Video games set in the 1520s");
        AddCategoryUrlFile("Video games set in the 1530s");
        AddCategoryUrlFile("Video games set in the 1540s");
        AddCategoryUrlFile("Video games set in the 1560s");
        AddCategoryUrlFile("Video games set in the 1570s");
        AddCategoryUrlFile("Video games set in the 1580s");
        AddCategoryUrlFile("Video games set in the 1590s");
        AddCategoryUrlFile("Video games set in 16th-century Ottoman Empire");
        AddCategoryUrlFile("Video games set in 16th-century Sengoku period");
        AddCategoryUrlFile("Samurai Warriors");
        AddCategoryUrlFile("Tenchu games");

        var ps2 = new Platform("Sony Playstation 2") { SpecificationId = "sony_playstation_2" };
        db.Platforms.Add(ps2);

        db.Games.Add(new Game("Red Ninja") { PlatformIds = [ps2.Id], Links = [new("Wikipedia", "https://en.wikipedia.org/wiki/Red_Ninja:_End_of_Honor")] });
        db.Games.Add("Ninja Master's");
        db.Games.Add("Nioh 2 – The Complete Edition");

        bi.ImportGameProperty();

        Assert.All(db.Games, g => g.TagIds.Single());
        Assert.All(db.Games, g => g.Links.Single());
        Assert.Single(db.Tags, t => t.Name == "Video games set in the 16th century");
        downloader.AssertAllUrlsCalledOnce();
    }

    [Fact]
    public void TwentyFifthCenturyImports()
    {
        var search = "video games set in the 25th century";
        var bi = Setup(search, out var db, out var downloader, out var api);

        downloader.FilesByUrl.Add(api.GetSearchUrl(search, WikipediaNamespace.Category), "Resources/25th-century/search.json");
        downloader.FilesByUrl.Add(api.GetCategoryMembersUrl("Category:Video games set in the 25th century"), "Resources/25th-century/category.json");

        db.Games.Add("Beyond Good & Evil");
        db.Games.Add(new Game("Beyond Good & Evil") { Links = [new("Wikipedia", "https://en.wikipedia.org/wiki/Beyond_Good_%26_Evil_(video_game)")] });
        db.Games.Add("Æon Flux");
        db.Games.Add("Aeon Flux");
        db.Games.Add(new Game("Æon Flux") { Links = [new("Wikipedia", "https://en.wikipedia.org/wiki/%C3%86on_Flux_(video_game)")] });

        bi.ImportGameProperty();
        
        Assert.All(db.Games, g => g.TagIds.Single());
        Assert.All(db.Games, g => g.Links.Single());
        Assert.Single(db.Tags, t => t.Name == "Video games set in the 25th century");
        downloader.AssertAllUrlsCalledOnce();
    }
}