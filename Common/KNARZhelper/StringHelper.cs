using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace KNARZhelper
{
    /// <summary>
    /// Helper class with extensions for the string data type
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Dictionary with special characters that need to be replaced before regular removing of diacritics.
        /// </summary>
        public static IReadOnlyDictionary<string, string> SpecialDiacritics = new Dictionary<string, string>
        {
            {
                "ß".Normalize(NormalizationForm.FormD), "ss".Normalize(NormalizationForm.FormD)
            },
        };

        /// <summary>
        /// Replaces multiple whitespaces between characters with a single one and trims them from
        /// beginning and end of the string.
        /// </summary>
        /// <param name="str">String to process</param>
        /// <returns>String with collapsed whitespaces</returns>
        public static string CollapseWhitespaces(this string str) => Regex.Replace(str, @"\s+", " ").Trim();

        /// <summary>
        /// Checks, if a string contains a substring
        /// </summary>
        /// <param name="source">String that will be checked</param>
        /// <param name="toCheck">The substring, that will be searched</param>
        /// <param name="comp">StringComparer to allow ignore case etc.</param>
        /// <returns>true, if the string contains the substring</returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp) => source?.IndexOf(toCheck, comp) >= 0;

        /// <summary>
        /// Substitutes every digit to its counterpart in roman notation.
        /// </summary>
        /// <param name="str">String to process</param>
        /// <returns>String with roman numerals</returns>
        public static string DigitsToRomanNumbers(this string str) => str.Replace("1", "I")
            .Replace("2", "II")
            .Replace("3", "III")
            .Replace("4", "IV")
            .Replace("5", "V")
            .Replace("6", "VI")
            .Replace("7", "VII")
            .Replace("8", "VIII")
            .Replace("9", "IX");

        /// <summary>
        /// Escapes a string to be used in a URL.
        /// </summary>
        /// <param name="str">String to escape</param>
        /// <returns>Escaped string</returns>
        public static string EscapeDataString(this string str) => Uri.EscapeDataString(str);

        /// <summary>
        /// Escapes quotes in a string to \".
        /// </summary>
        /// <param name="str">String to escape</param>
        /// <returns>Escaped string</returns>
        public static string EscapeQuotes(this string str) => str.Replace("\"", "\\\"");

        /// <summary>
        /// Checks if a string is a valid HTTP or HTTPS URL.
        /// </summary>
        /// <param name="url">The URL to check</param>
        /// <returns>True if the URL is valid, false otherwise</returns>
        public static bool IsValidHttpUrl(this string url) => Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        /// <summary>
        /// Checks if a string matches a regular expression.
        /// </summary>
        /// <param name="source">String to check</param>
        /// <param name="regEx">Regular expression to match</param>
        /// <returns>True if the string matches the regular expression, false otherwise</returns>
        public static bool RegExIsMatch(this string source, string regEx)
        {
            var regExOptions = RegexOptions.ExplicitCapture;
            regExOptions |= RegexOptions.IgnoreCase;
            regExOptions |= RegexOptions.Compiled;
            return Regex.IsMatch(source, regEx, regExOptions);
        }

        /// <summary>
        /// Removes diacritics from a string
        /// </summary>
        /// <param name="str">String to process</param>
        /// <returns>String without diacritics</returns>
        public static string RemoveDiacritics(this string str)
        {
            var stringBuilder = new StringBuilder(str.Normalize(NormalizationForm.FormD));

            // Replace certain special chars with special combinations of ASCII chars (e.g. German
            // double s)
            foreach (var keyValuePair in SpecialDiacritics)
            {
                stringBuilder.Replace(keyValuePair.Key, keyValuePair.Value);
            }

            for (var i = 0; i < stringBuilder.Length; i++)
            {
                var c = stringBuilder[i];

                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Remove(i, 1);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Removes stuff like "Special Edition" or HD from the end of a game name.
        /// </summary>
        /// <param name="str">string to process</param>
        /// <returns>string without the edition suffix</returns>
        public static string RemoveEditionSuffix(this string str)
        {
            var regExOptions = RegexOptions.ExplicitCapture;
            regExOptions |= RegexOptions.Compiled;
            var ignoredEndWordsRegex = new Regex(@"(\s*[:-])?(\s+([a-z']+\s+(edition|cut)|hd|collection|remaster(ed)?|remake|ultimate|anthology|game of the|deluxe))+$", regExOptions | RegexOptions.IgnoreCase);
            var match = ignoredEndWordsRegex.Match(str);

            return match.Success ? str.Remove(match.Index).Trim() : str;
        }

        /// <summary>
        /// Removes the first occurrence of a string from the source string
        /// </summary>
        /// <param name="source">string to process</param>
        /// <param name="remove">string to removed</param>
        /// <returns>processed string</returns>
        public static string RemoveFirst(this string source, string remove)
        {
            var index = source.IndexOf(remove, StringComparison.OrdinalIgnoreCase);
            return (index < 0)
                ? source
                : source.Remove(index, remove.Length);
        }

        /// <summary>
        /// Removes all characters that aren't part of the alphabet, a number, a whitespace or a hyphen.
        /// </summary>
        /// <param name="str">String to process</param>
        /// <param name="replaceStr">String to replace removed characters with</param>
        /// <returns>Processed string</returns>
        public static string RemoveSpecialChars(this string str, string replaceStr = "") => Regex.Replace(str, @"[^a-zA-Z0-9\-\s]+", replaceStr);

        /// <summary>
        /// Removes all values from a string that are between two substrings like parentheses or
        /// html tags.
        /// </summary>
        /// <param name="str">The string with parts to be removed</param>
        /// <param name="from">
        /// The substring or character marking the beginning of the text, that will be removed
        /// </param>
        /// <param name="to">
        /// The substring or character marking the end of the text, that will be removed
        /// </param>
        /// <returns>String without the unwanted substrings</returns>
        public static string RemoveTextBetween(this string str, string from, string to)
        {
            var lengthTo = to.Length;

            while (str.IndexOf(from, StringComparison.Ordinal) > -1)
            {
                str = str.Remove(str.IndexOf(from, StringComparison.Ordinal),
                    str.IndexOf(to, StringComparison.Ordinal) - str.IndexOf(from, StringComparison.Ordinal) + lengthTo);
            }

            return str;
        }

        /// <summary>
        /// Converts a string to title case (first letter of each word capitalized).
        /// </summary>
        /// <param name="title">String to convert</param>
        /// <returns>Title case string</returns>
        public static string ToTitleCase(this string title) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());

        /// <summary>
        /// Encodes an URL.
        /// </summary>
        /// <param name="str">String to encode</param>
        /// <returns>Encoded string</returns>
        public static string UrlEncode(this string str) => HttpUtility.UrlEncode(str);
    }
}