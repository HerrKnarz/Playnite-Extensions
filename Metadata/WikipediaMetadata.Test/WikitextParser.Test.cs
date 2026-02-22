using System.Linq;
using WikipediaMetadata.Test.Fakes;
using Xunit;

namespace WikipediaMetadata.Test;

public class WikitextParserTest
{
    [Theory]
    [InlineData("Day_of_the_Tentacle", "https://upload.wikimedia.org/wikipedia/en/7/79/Day_of_the_Tentacle_artwork.jpg")]
    public void TestGetImageUrl(string key, string expectedResult)
    {
        var webclient = new FakeWebClient(WikipediaApiUrl.GetPagePropertiesUrl(key), "data/WikiTextParser/dott-pageproperties.json");
        var originalImageUrl = new WikipediaApi(webclient).GetPageProperties(key)?.Query?.Pages?.FirstOrDefault()?.Original?.Source;
        Assert.Equal(expectedResult, originalImageUrl);
    }

    [Theory]
    [InlineData("[Test]\n[Second]", "[test]")]
    [InlineData("[Test]<!--comment!-->", "[test]")]
    [InlineData("[Test]\n<!--comment!-->[Second]", "[test]")]
    [InlineData("[Test]<!--comment!-->\n[Second]", "[test]")]
    public void TestCleanTemplateName(string wikitext, string expectedResult)
    {
        Assert.Equal(expectedResult, WikitextParser.CleanTemplateName(wikitext));
    }
}
