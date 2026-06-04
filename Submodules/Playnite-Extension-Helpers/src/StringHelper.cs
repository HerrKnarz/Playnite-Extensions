using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PlayniteExtensionHelpers;

/// <summary>
/// Helper class with extensions for the string data type
/// </summary>
public static partial class StringHelper
{
    /// <summary>
    /// Dictionary with special characters that need to be replaced before regular removing of diacritics.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> SpecialDiacritics = new Dictionary<string, string>
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
    public static string? CollapseWhitespaces(this string? str) => str.IsNullOrEmpty() ? str : WhiteSpaceRegex().Replace(str, " ").Trim();

    /// <summary>
    /// Checks, if a string contains a substring
    /// </summary>
    /// <param name="source">String that will be checked</param>
    /// <param name="toCheck">The substring, that will be searched</param>
    /// <param name="comp">StringComparer to allow ignore case etc.</param>
    /// <returns>true, if the string contains the substring</returns>
    public static bool Contains(this string? source, string toCheck, StringComparison comp) => source?.IndexOf(toCheck, comp) >= 0;

    /// <summary>
    /// Substitutes every digit to its counterpart in roman notation.
    /// </summary>
    /// <param name="str">String to process</param>
    /// <returns>String with roman numerals</returns>
    public static string? DigitsToRomanNumbers(this string? str) => str.IsNullOrEmpty() ? str : str
        .Replace("1", "I")
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
    public static string? EscapeDataString(this string? str) => str.IsNullOrEmpty() ? str : Uri.EscapeDataString(str);

    /// <summary>
    /// Escapes quotes in a string to \".
    /// </summary>
    /// <param name="str">String to escape</param>
    /// <returns>Escaped string</returns>
    public static string? EscapeQuotes(this string? str) => str.IsNullOrEmpty() ? str : str.Replace("\"", "\\\"");

    /// <summary>
    /// Formats the specified string according to the options provided in the format parameters.
    /// </summary>
    /// <remarks>
    /// The formatting options are applied in a specific order. Some options may override or
    /// interact with others; for example, whitespace transformations are applied after other
    /// character replacements. If multiple options are enabled that affect the same characters, the
    /// final result reflects the cumulative effect of all enabled options.
    /// </remarks>
    /// <param name="str">The input string to be formatted.</param>
    /// <param name="formatParams">
    /// An object specifying the formatting options to apply, such as removing hyphens, converting
    /// to title case, or replacing invalid file name characters. Cannot be null.
    /// </param>
    /// <returns>
    /// A new string that results from applying the specified formatting options to the input string.
    /// </returns>
    public static string? FormatString(this string? str, StringFormatParameters formatParams)
    {
        if (str.IsNullOrEmpty())
        {
            return str;
        }

        if (formatParams.RemoveEditionSuffix)
        {
            str = str.RemoveEditionSuffix();
        }

        if (formatParams.RemoveHyphens)
        {
            str = str?.Replace("-", "");
        }

        if (formatParams.UnderscoresToWhitespaces)
        {
            str = str?.Replace("_", " ");
        }

        if (formatParams.RemoveSpecialChars)
        {
            str = str?.RemoveSpecialChars();
        }

        if (formatParams.RemoveDiacritics)
        {
            str = str?.RemoveDiacritics();
        }

        if (formatParams.ToTitleCase)
        {
            str = str?.ToTitleCase();
        }

        if (formatParams.ToLower)
        {
            str = str?.ToLower();
        }

        str = formatParams.RemoveWhitespaces ? str?.Replace(" ", "") : str?.CollapseWhitespaces();

        if (formatParams.WhitespacesToHyphens)
        {
            str = str?.Replace(" ", "-");
        }

        if (formatParams.WhitespacesToUnderscores)
        {
            str = str?.Replace(" ", "_");
        }

        if (formatParams.ReplaceInvalidFileNameChars)
        {
            str = str.ReplaceInvalidFileNameChars(formatParams.InvalidCharReplacement ?? string.Empty);
        }

        if (formatParams.EscapeDataString)
        {
            str = str.EscapeDataString();
        }

        if (formatParams.UrlEncode)
        {
            str = str.UrlEncode();
        }

        return str;
    }

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? source) => string.IsNullOrEmpty(source);

    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? source) => string.IsNullOrWhiteSpace(source);

    /// <summary>
    /// Checks if a string is a valid HTTP or HTTPS URL.
    /// </summary>
    /// <param name="url">The URL to check</param>
    /// <returns>True if the URL is valid, false otherwise</returns>
    public static bool IsValidHttpUrl(this string? url) => Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    /// <summary>
    /// Normalizes a search term by removing special characters, converting to lowercase, and
    /// eliminating spaces and hyphens.
    /// </summary>
    /// <remarks>
    /// Normalization helps ensure consistent search behavior by standardizing input format. Use
    /// this method before performing search comparisons to reduce mismatches caused by formatting differences.
    /// </remarks>
    /// <param name="searchTerm">
    /// The search term to normalize. Cannot be null; must be a non-empty string.
    /// </param>
    /// <returns>
    /// A normalized string suitable for search operations. Returns an empty string if the input
    /// contains only removable characters.
    /// </returns>
    public static string? NormalizeSearchTerm(this string? searchTerm) => searchTerm.RemoveSpecialChars()?.ToLower().Replace(" ", "").Replace("-", "");

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
    public static string? RemoveDiacritics(this string? str)
    {
        if (str.IsNullOrEmpty())
        {
            return str;
        }

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
    public static string? RemoveEditionSuffix(this string? str)
    {
        if (str.IsNullOrEmpty())
        {
            return str;
        }

        var regExOptions = RegexOptions.ExplicitCapture;
        regExOptions |= RegexOptions.Compiled;
        var ignoredEndWordsRegex = new Regex(@"(\s*[:-])?(\s+([a-z']+\s+(edition|cut)|hd|collection|remaster(ed)?|remake|ultimate|anthology|game of the|deluxe))+$", regExOptions | RegexOptions.IgnoreCase);
        var match = ignoredEndWordsRegex.Match(str);

        return match.Success ? str[..match.Index].Trim() : str;
    }

    /// <summary>
    /// Removes the first occurrence of a string from the source string
    /// </summary>
    /// <param name="source">string to process</param>
    /// <param name="remove">string to removed</param>
    /// <returns>processed string</returns>
    public static string? RemoveFirst(this string? source, string remove)
    {
        if (source.IsNullOrEmpty())
        {
            return source;
        }

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
    public static string? RemoveSpecialChars(this string? str, string replaceStr = "") => str.IsNullOrEmpty() ? str : SpecialCharRegex().Replace(str, replaceStr);

    /// <summary>
    /// Removes all values from a string that are between two substrings like parentheses or html tags.
    /// </summary>
    /// <param name="str">The string with parts to be removed</param>
    /// <param name="from">
    /// The substring or character marking the beginning of the text, that will be removed
    /// </param>
    /// <param name="to">The substring or character marking the end of the text, that will be removed</param>
    /// <returns>String without the unwanted substrings</returns>
    public static string? RemoveTextBetween(this string? str, string from, string to)
    {
        if (str.IsNullOrEmpty())
        {
            return str;
        }

        var lengthTo = to.Length;

        while (str.IndexOf(from, StringComparison.Ordinal) > -1)
        {
            str = str.Remove(str.IndexOf(from, StringComparison.Ordinal),
                str.IndexOf(to, StringComparison.Ordinal) - str.IndexOf(from, StringComparison.Ordinal) + lengthTo);
        }

        return str;
    }

    /// <summary>
    /// Replaces all invalid file name characters in the specified string with the provided
    /// replacement string.
    /// </summary>
    /// <remarks>
    /// Invalid file name characters are determined by <see
    /// cref="System.IO.Path.GetInvalidFileNameChars"/>. This method does not validate the resulting
    /// string as a file name.
    /// </remarks>
    /// <param name="str">The input string to process. Cannot be null.</param>
    /// <param name="replaceStr">
    /// The string to use as a replacement for each invalid file name character. If not specified,
    /// invalid characters are removed.
    /// </param>
    /// <returns>
    /// A string with all invalid file name characters replaced by the specified replacement string.
    /// </returns>
    public static string? ReplaceInvalidFileNameChars(this string? str, string replaceStr = "") => str.IsNullOrEmpty() ? str : string.Join(replaceStr, str.Split(Path.GetInvalidFileNameChars()));

    /// <summary>
    /// Strips query parameters and fragments from a URL, returning only the base path. For example,
    /// "https://example.com/page?query=123#section" would be stripped down to "https://example.com/page".
    /// </summary>
    /// <param name="str">The input string to process. Cannot be null.</param>
    /// <returns>Input string with query parameters and fragments removed</returns>
    public static string? StripUriParams(this string? str) => str.IsNullOrEmpty() ? str : new Uri(str).GetLeftPart(UriPartial.Path);

    /// <summary>
    /// Converts a string to title case (first letter of each word capitalized).
    /// </summary>
    /// <param name="title">String to convert</param>
    /// <returns>Title case string</returns>
    public static string? ToTitleCase(this string? title) => title.IsNullOrEmpty() ? title : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());

    /// <summary>
    /// Encodes an URL.
    /// </summary>
    /// <param name="str">String to encode</param>
    /// <returns>Encoded string</returns>
    public static string? UrlEncode(this string? str) => str.IsNullOrEmpty() ? str : HttpUtility.UrlEncode(str);

    [GeneratedRegex(@"[^a-zA-Z0-9\-\s]+", RegexOptions.IgnoreCase)]
    private static partial Regex SpecialCharRegex();

    [GeneratedRegex(@"\s+", RegexOptions.IgnoreCase)]
    private static partial Regex WhiteSpaceRegex();
}

/// <summary>
/// Represents a set of options that control how string formatting operations are performed.
/// </summary>
/// <remarks>
/// Use this class to specify various transformations and sanitation behaviors when formatting
/// strings, such as removing diacritics, replacing invalid characters, or applying case
/// conversions. Each property enables or disables a specific formatting rule. This class is
/// typically used to configure string formatting utilities or helpers that require customizable behavior.
/// </remarks>
public class StringFormatParameters
{
    /// <summary>
    /// Indicates whether data strings should be escaped using percent-encoding.
    /// </summary>
    public bool EscapeDataString { get; set; } = false;

    /// <summary>
    /// String that will replace invalid file name characters when the ReplaceInvalidFileNameChars
    /// option is enabled. If null, invalid characters will be removed instead of replaced.
    /// </summary>
    public string? InvalidCharReplacement { get; set; } = null;

    /// <summary>
    /// Indicates whether diacritical marks (accents) should be removed from characters in the
    /// string. For example, "é" would be transformed to "e". This can be useful for creating more
    /// standardized or ASCII-only strings.
    /// </summary>
    public bool RemoveDiacritics { get; set; } = false;

    /// <summary>
    /// Indicates whether the edition suffix should be removed from the string. Is typically used
    /// for game names that include suffixes like "Deluxe Edition" or "Game of the Year Edition",
    /// where the edition information is not desired in the formatted output.
    /// </summary>
    public bool RemoveEditionSuffix { get; set; } = false;

    /// <summary>
    /// Indicates whether hyphens should be removed from the processed text.
    /// </summary>
    public bool RemoveHyphens { get; set; } = false;

    /// <summary>
    /// Indicates whether special characters are removed during processing.
    /// </summary>
    public bool RemoveSpecialChars { get; set; } = false;

    /// <summary>
    /// Indicates whether whitespace characters should be removed during processing.
    /// </summary>
    public bool RemoveWhitespaces { get; set; } = false;

    /// <summary>
    /// Indicates whether invalid characters in file names are automatically replaced.
    /// </summary>
    public bool ReplaceInvalidFileNameChars { get; set; } = false;

    /// <summary>
    /// Indicates whether the string should be converted to lowercase.
    /// </summary>
    public bool ToLower { get; set; } = false;

    /// <summary>
    /// Indicates whether the string should be converted to title case, where the first letter of
    /// each word is capitalized and the rest are lowercase.
    /// </summary>
    public bool ToTitleCase { get; set; } = false;

    /// <summary>
    /// Indicates whether underscores in the string should be replaced with whitespace characters.
    /// </summary>
    public bool UnderscoresToWhitespaces { get; set; } = false;

    /// <summary>
    /// Indicates whether the string should be URL-encoded.
    /// </summary>
    public bool UrlEncode { get; set; } = false;

    /// <summary>
    /// Indicates whether whitespace characters in the string should be replaced with hyphens.
    /// </summary>
    public bool WhitespacesToHyphens { get; set; } = false;

    /// <summary>
    /// Indicates whether whitespace characters in the string should be replaced with underscores.
    /// </summary>
    public bool WhitespacesToUnderscores { get; set; } = false;
}