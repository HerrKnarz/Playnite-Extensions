using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace WikipediaMetadata.Test
{
    public class WikitextParserTest
    {


        [Theory]
        [InlineData("Day_of_the_Tentacle", "https://upload.wikimedia.org/wikipedia/en/7/79/Day_of_the_Tentacle_artwork.jpg")]
        public void TestGetImageUrl(string key, string expectedResult)
        {
            Assert.Equal(expectedResult, WikipediaHelper.GetImageUrl(key));
        }


        [Theory]
        [InlineData("[Test]\n[Second]", "[test]")]
        [InlineData("[Test]<!--comment!-->", "[test]")]
        [InlineData("[Test]\n<!--comment!-->[Second]", "[test]")]
        [InlineData("[Test]<!--comment!-->\n[Second]", "[test]")]
        public void TestCleanTemplateName(string wikitext, string expectedResult)
        {
            WikitextParser wikitextParser = new WikitextParser(null);

            Assert.Equal(expectedResult, wikitextParser.CleanTemplateName(wikitext));
        }

        ResolveEventHandler eventHandler = (sender, args) =>
        {
            // Get just the name of assmebly
            // Aseembly name excluding version and other metadata
            string name = new Regex(",.*").Replace(args.Name, string.Empty);

            // Load whatever version available
            return Assembly.Load(name);
        };
    }
}
