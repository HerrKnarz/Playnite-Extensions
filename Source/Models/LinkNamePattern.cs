using LinkUtilities.Helper;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace LinkUtilities.Models
{
    /// <summary>
    /// Pattern to find the right link name for a given URL and link title combination
    /// </summary>
    public class LinkNamePattern
    {
        /// <summary>
        /// Name to use for the link if the patterns match
        /// </summary>
        [JsonProperty("linkName")]
        public string LinkName { get; set; }

        /// <summary>
        /// Pattern the URL has to match. Can contain wildcards * (zero or more characters) or ? (exactly one character).
        /// </summary>
        [JsonProperty("urlPattern")]
        public string UrlPattern { get; set; }

        /// <summary>
        /// Pattern the link title has to match. Can contain wildcards * (zero or more characters) or ? (exactly one character).
        /// </summary>
        [JsonProperty("namePattern")]
        public string NamePattern { get; set; }

        /// <summary>
        /// Regular expression of the UrlPattern
        /// </summary>
        [JsonIgnore]
        public string UrlRegEx { get => ParseHelper.WildCardToRegular(UrlPattern); }

        /// <summary>
        /// Regular expression of the NamePattern
        /// </summary>
        [JsonIgnore]
        public string NameRegEx { get => ParseHelper.WildCardToRegular(NamePattern); }

        public bool LinkMatch(string linkName, string linkUrl)
        {
            return (NamePattern == string.Empty || Regex.IsMatch(linkName, NameRegEx)) &&
                (UrlPattern == string.Empty || Regex.IsMatch(linkUrl, UrlRegEx));
        }
    }
}
