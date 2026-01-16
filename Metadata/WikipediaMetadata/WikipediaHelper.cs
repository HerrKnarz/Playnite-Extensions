using System.Linq;
using System.Text.RegularExpressions;

namespace WikipediaMetadata;

internal static class WikipediaHelper
{
    /// <summary>
    /// Fetches the url of the main image from the page.
    /// </summary>
    /// <param name="key">Key of the page</param>
    /// <returns>Url of the image</returns>
    internal static string GetImageUrl(string key) =>
        WikipediaApiCaller.GetImage(key)?.Query?.Pages.FirstOrDefault()?.Original?.Source;

    private static readonly Regex TitleParenthesesRegex = new(@"^(?<title>.+) \((?<parenContents>.+)\)$", RegexOptions.Compiled);

    extension(string articleName)
    {
        public string StripCategoryPrefix() => articleName?.Split([':'], 2).Last();

        public bool IsArticleAboutGame(out string displayTitle)
        {
            displayTitle = articleName;
            var match = TitleParenthesesRegex.Match(articleName);
            if (match.Success)
            {
                var parenContents = match.Groups["parenContents"].Value;
                if (parenContents.Contains("comic") || parenContents.Contains("soundtrack") || parenContents.Contains("film") || parenContents.Contains("novel"))
                    return false;
            }

            displayTitle = match.Success ? match.Groups["title"].Value : articleName;
            return true;
        }
    }
}
