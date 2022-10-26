using System.Net;
using System.Text.RegularExpressions;

namespace LinkUtilities.Helper
{
    public static class StringHelper
    {
        public static string RemoveSpecialChars(this string str)
        {
            return Regex.Replace(str, @"[^a-zA-Z0-9\-\s]+", "");
        }

        public static string CollapseWhitespaces(this string str)
        {
            return Regex.Replace(str, @"\s+", " ").Trim();
        }

        public static string EscapeDataString(this string str)
        {
            return System.Uri.EscapeDataString(str);
        }

        public static string UrlEncode(this string str)
        {
            return WebUtility.UrlEncode(str);
        }




    }
}
