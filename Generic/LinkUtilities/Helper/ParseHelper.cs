using HtmlAgilityPack;
using KNARZhelper;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace LinkUtilities.Helper
{
    /// <summary>
    ///     Helper functions for parsing strings
    /// </summary>
    internal static class ParseHelper
    {
        /// <summary>
        ///     Gets a JSON result from an API and deserializes it.
        /// </summary>
        /// <typeparam name="T">Type the JSON gets deserialized to</typeparam>
        /// <param name="apiUrl">Url to fetch the JSON result from</param>
        /// <param name="linkName">Link name for the error message</param>
        /// <param name="encoding">the encoding to use</param>
        /// <returns>Deserialized JSON result</returns>
        internal static T GetJsonFromApi<T>(string apiUrl, string linkName, Encoding encoding = null)
        {
            try
            {
                if (encoding is null)
                {
                    encoding = Encoding.Default;
                }

                var client = new WebClient { Encoding = encoding };

                client.Headers.Add("Accept", "application/json");
                client.Headers.Add("user-agent", "Playnite LinkUtilities AddOn");

                return JsonConvert.DeserializeObject<T>(client.DownloadString(apiUrl));
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {linkName}");
            }

            return default;
        }

        /// <summary>
        ///     Gets the search results from a mediawiki website via opensearch.
        /// </summary>
        /// <param name="searchUrl">URL of the search page with {0} for the search term</param>
        /// <param name="searchTerm">Term to search for. Will be encoded in the function!</param>
        /// <param name="linkName">Name of the site for the error message</param>
        /// <returns>Search results for the search dialogs. Will be an empty list in case of an error.</returns>
        internal static List<SearchResult> GetMediaWikiResultsFromApi(string searchUrl, string searchTerm, string linkName)
        {
            var result = new List<SearchResult>();

            try
            {
                var client = new WebClient();

                client.Headers.Add("Accept", "application/xml");

                var xml = client.DownloadString(string.Format(searchUrl, searchTerm.UrlEncode()));

                var searchResults = xml.ParseXml<SearchSuggestion>();

                result.AddRange(searchResults.Section.Select(item => new SearchResult
                {
                    Name = item.Text.Value,
                    Url = item.Url.Value,
                    Description = item.Url.Value
                }));
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {linkName}");
            }

            return result;
        }

        /// <summary>
        ///     Scrapes the search results from a mediawiki search results page. Is used on sites, where opensearch doesn't return
        ///     sufficient results.
        /// </summary>
        /// <param name="searchUrl">URL of the search page with {0} for the search term</param>
        /// <param name="searchTerm">Term to search for. Will be encoded in the function!</param>
        /// <param name="websiteUrl">Base URL to add the relative URL of the search results to</param>
        /// <param name="linkName">Name of the site for the error message</param>
        /// <param name="slashCount">Minimum of slashes in the relative URL to indicate, if it's a subpage</param>
        /// <returns>Search results for the search dialogs. Will be an empty list in case of an error.</returns>
        internal static List<SearchResult> GetMediaWikiResultsFromHtml(string searchUrl, string searchTerm, string websiteUrl, string linkName, int slashCount = 3)
        {
            var result = new List<SearchResult>();

            try
            {
                var web = new HtmlWeb();
                var doc = web.Load(string.Format(searchUrl, searchTerm.UrlEncode()));

                var resultNode = doc.DocumentNode.SelectSingleNode("//div[@class='searchresults']");

                if (resultNode.SelectSingleNode("./h2/span[text() = 'Page title matches']") != null)
                {
                    var htmlNodes = resultNode.SelectSingleNode("./ul[@class='mw-search-results']").SelectNodes("./li[@class='mw-search-result']");

                    if (htmlNodes?.Any() ?? false)
                    {
                        foreach (var node in htmlNodes)
                        {
                            var url = node.SelectSingleNode("./div[@class='mw-search-result-heading']/a").GetAttributeValue("href", "");

                            // MediaWiki returns subpages to games in the results, so we simply count the slashes to filter them out.
                            if (url.Count(x => x == '/') >= slashCount)
                            {
                                continue;
                            }

                            var redirect = string.Empty;
                            var resultText = node.SelectSingleNode("./div[@class='searchresult']");
                            if (resultText != null)
                            {
                                redirect = resultText.InnerText.StartsWith("#REDIRECT") ? "(REDIRECT) " : string.Empty;
                            }

                            result.Add(new SearchResult
                            {
                                Name = WebUtility.HtmlDecode(node.SelectSingleNode("./div[@class='mw-search-result-heading']").InnerText),
                                Url = websiteUrl + url,
                                Description = redirect + websiteUrl + url
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {linkName}");
            }

            return result;
        }

        /// <summary>
        ///     Deserializes an XML string into an object
        /// </summary>
        /// <typeparam name="T">Object the XML will be deserialized to.</typeparam>
        /// <param name="this">XML string</param>
        /// <returns>The deserialized XML</returns>
        internal static T ParseXml<T>(this string @this) where T : class
        {
            var reader = XmlReader.Create(@this.Trim().ToStream(), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Document });
            return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
        }

        /// <summary>
        ///     Converts a string to a stream.
        /// </summary>
        /// <param name="this">String to convert</param>
        /// <returns>Stream from the string</returns>
        internal static Stream ToStream(this string @this)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(@this);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        ///     Converts a wildcard pattern to a regular expression.
        ///     * is interpreted as zero or more characters,
        ///     ? is interpreted as exactly one character.
        /// </summary>
        /// <param name="value">Pattern to convert</param>
        /// <returns>The resulting regular expression</returns>
        internal static string WildCardToRegular(string value) => "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
    }
}