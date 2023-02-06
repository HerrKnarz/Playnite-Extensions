using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LinkUtilities.Helper
{
    /// <summary>
    /// Helper class with extensions for the string data type
    /// </summary>
    internal static class StringHelper
    {
        /// <summary>
        /// Removes all characters that aren't part of the alphabet, a number, a whitespace or a hyphen.
        /// </summary>
        public static string RemoveSpecialChars(this string str)
        {
            return Regex.Replace(str, @"[^a-zA-Z0-9\-\s]+", "");
        }

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
        public static string CollapseWhitespaces(this string str)
        {
            return Regex.Replace(str, @"\s+", " ").Trim();
        }

        /// <summary>
        /// Escapes a string to be used in a URL.
        /// </summary>
        public static string EscapeDataString(this string str)
        {
            return System.Uri.EscapeDataString(str);
        }

        /// <summary>
        /// Encodes an URL.
        /// </summary>
        public static string UrlEncode(this string str)
        {
            return WebUtility.UrlEncode(str);
        }

        /// <summary>
        /// Escapes quotes in a string to \".
        /// </summary>
        public static string EscapeQuotes(this string str)
        {
            return str.Replace("\"", "\\\"");
        }

        public static string ToTitleCase(this string title)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }

        /// <summary>
        /// Substitutes every digit to its counterpart in roman notation.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string DigitsToRomanNumbers(this string str)
        {
            return str.Replace("1", "I")
                .Replace("2", "II")
                .Replace("3", "III")
                .Replace("4", "IV")
                .Replace("5", "V")
                .Replace("6", "VI")
                .Replace("7", "VII")
                .Replace("8", "VIII")
                .Replace("9", "IX");
        }
    }
}
