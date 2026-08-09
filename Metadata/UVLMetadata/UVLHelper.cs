using System.Linq;
using System.Text.RegularExpressions;

namespace UVLMetadata;

internal static class UVLHelper
{
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
