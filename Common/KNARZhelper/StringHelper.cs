using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace KNARZhelper
{
    /// <summary>
    /// Helper class with extensions for the string data type
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Removes all characters that aren't part of the alphabet, a number, a whitespace or a hyphen.
        /// </summary>
        public static string RemoveSpecialChars(this string str) => Regex.Replace(str, @"[^a-zA-Z0-9\-\s]+", "");

        /// <summary>
        /// Dictionary with special characters that need to be replaced before regular removing of diacritics. 
        /// </summary>
        public static IReadOnlyDictionary<string, string> SPECIAL_DIACRITICS = new Dictionary<string, string>
                                                                   {
                                                                        { "ß".Normalize(NormalizationForm.FormD), "ss".Normalize(NormalizationForm.FormD) },
                                                                   };

        /// <summary>
        /// Removes diacritics from a string
        /// </summary>
        public static string RemoveDiacritics(this string str)
        {
            StringBuilder stringBuilder = new StringBuilder(str.Normalize(NormalizationForm.FormD));

            // Replace certain special chars with special combinations of ASCII chars (e.g. German double s)
            foreach (KeyValuePair<string, string> keyValuePair in SPECIAL_DIACRITICS)
            {
                stringBuilder.Replace(keyValuePair.Key, keyValuePair.Value);
            }

            for (int i = 0; i < stringBuilder.Length; i++)
            {
                char c = stringBuilder[i];

                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Remove(i, 1);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Replaces multiple whitespaces between characters with a single one and trims them from beginning and end of the string.
        /// </summary>
        public static string CollapseWhitespaces(this string str) => Regex.Replace(str, @"\s+", " ").Trim();

        /// <summary>
        /// Escapes a string to be used in a URL.
        /// </summary>
        public static string EscapeDataString(this string str) => System.Uri.EscapeDataString(str);

        /// <summary>
        /// Encodes an URL.
        /// </summary>
        public static string UrlEncode(this string str) => WebUtility.UrlEncode(str);

        /// <summary>
        /// Escapes quotes in a string to \".
        /// </summary>
        public static string EscapeQuotes(this string str) => str.Replace("\"", "\\\"");

        public static string ToTitleCase(this string title) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());

        /// <summary>
        /// Substitutes every digit to its counterpart in roman notation.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
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
        /// Removes all values from a string that are between two substrings like parentheses or html tags.
        /// </summary>
        /// <param name="str">The string with parts to be removed</param>
        /// <param name="from">The substring or character marking the beginning of the text, that will be removed</param>
        /// <param name="to">The substring or character marking the end of the text, that will be removed</param>
        /// <returns>String without the unwanted substrings</returns>
        public static string RemoveTextBetween(this string str, string from, string to)
        {
            int lengthTo = to.Length;

            while (str.IndexOf(from) > -1)
            {
                str = str.Remove(str.IndexOf(from), str.IndexOf(to) - str.IndexOf(from) + lengthTo);
            }

            return str;
        }

        /// <summary>
        /// Removes stuff like "Special Edition" or HD from the end of a game name.
        /// </summary>
        /// <param name="str">string to process</param>
        /// <returns>string without the edition suffix</returns>
        public static string RemoveEditionSuffix(this string str)
        {
            RegexOptions regExOptions = RegexOptions.ExplicitCapture;
            regExOptions |= RegexOptions.Compiled;
            Regex ignoredEndWordsRegex = new Regex(@"(\s*[:-])?(\s+([a-z']+\s+(edition|cut)|hd|collection|remaster(ed)?|remake|ultimate|anthology|game of the))+$", regExOptions | RegexOptions.IgnoreCase);
            Match match = ignoredEndWordsRegex.Match(str);

            if (match.Success)
            {
                return str.Remove(match.Index).Trim();
            }
            return str;
        }
    }
}