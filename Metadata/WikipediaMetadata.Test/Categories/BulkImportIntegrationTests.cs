using Playnite.SDK.Models;
using PlayniteExtensions.Common;
using PlayniteExtensions.Tests.Common;
using System.Linq;
using WikipediaCategories.BulkImport;
using WikipediaCategories.Tests.Fakes;
using Xunit;

namespace WikipediaCategories.Tests;

public class BulkImportIntegrationTests
{
    [Fact]
    public void SixteenthCenturyImports()
    {
        var search = "video games set in the 16th century";
        var db = new FakePlayniteDatabase();
        var ui = new FakeWikipediaBulkImportUserInterface(search, _ => true, _ => true);
        var downloader = new FakeWebDownloader();
        var api = new WikipediaApi(downloader, new(10, 47), "en");
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
        var bi = new WikipediaCategoryBulkImport(db, ui, new(api), new PlatformUtility(), 1);

        var ps2 = new Platform("Sony Playstation 2") { SpecificationId = "sony_playstation_2" };
        db.Platforms.Add(ps2);

        db.Games.Add(new Game("Red Ninja") { PlatformIds = [ps2.Id], Links = [new("Wikipedia", "https://en.wikipedia.org/wiki/Red_Ninja:_End_of_Honor")]});
        db.Games.Add("Ninja Master's");
        db.Games.Add("Nioh 2 – The Complete Edition");

        bi.ImportGameProperty();

        Assert.All(db.Games, g => g.TagIds.Single());
        Assert.All(db.Games, g => g.Links.Single());
        Assert.Single(db.Tags, t => t.Name == "Video games set in the 16th century");
        downloader.AssertAllUrlsCalledOnce();
    }
}
