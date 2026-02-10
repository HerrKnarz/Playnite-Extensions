using KNARZhelper;
using KNARZhelper.WebCommon;
using LinkUtilities.Models;
using LinkUtilities.Models.ApiResults;
using LinkUtilities.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace LinkUtilities.Helper
{
    /// <summary>
    /// Helper functions for parsing strings
    /// </summary>
    internal static class ParseHelper
    {
        /// <summary>
        /// Gets the search results from a mediawiki website via opensearch.
        /// </summary>
        /// <param name="searchUrl">URL of the search page with {0} for the search term</param>
        /// <param name="searchTerm">Term to search for. Will be encoded in the function!</param>
        /// <param name="linkName">Name of the site for the error message</param>
        /// <returns>
        /// Search results for the search dialogs. Will be an empty list in case of an error.
        /// </returns>
        internal static List<SearchResult> GetMediaWikiResultsFromApi(string searchUrl, string searchTerm, string linkName, BaseClasses.Linker linker = null)
        {
            var result = new List<SearchResult>();

            try
            {
                var xml = string.Empty;

                if (linker == null)
                {
                    var client = new WebClient();

                    client.Headers.Add("Accept", "application/xml");

                    xml = client.DownloadString(string.Format(searchUrl, searchTerm.UrlEncode()));
                }
                else
                {
                    var urlLoadResult = linker.Pipeline.LoadUrl(string.Format(searchUrl, searchTerm.UrlEncode()), DocumentType.Text, GlobalSettings.Instance().DebugMode);

                    if (urlLoadResult == null)
                    {
                        return result;
                    }

                    xml = urlLoadResult.PageText;

                    xml = xml.Replace("This XML file does not appear to have any style information associated with it. The document tree is shown below.", "");
                }

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
                Log.Error(ex, $"Error loading data from {linkName} - {searchUrl}");
            }

            return result;
        }

        /// <summary>
        /// Scrapes the search results from a mediawiki search results page. Is used on sites, where
        /// opensearch doesn't return sufficient results.
        /// </summary>
        /// <param name="searchUrl">URL of the search page with {0} for the search term</param>
        /// <param name="searchTerm">Term to search for. Will be encoded in the function!</param>
        /// <param name="websiteUrl">Base URL to add the relative URL of the search results to</param>
        /// <param name="linkName">Name of the site for the error message</param>
        /// <param name="slashCount">
        /// Minimum of slashes in the relative URL to indicate, if it's a subpage
        /// </param>
        /// <returns>
        /// Search results for the search dialogs. Will be an empty list in case of an error.
        /// </returns>
        internal static List<SearchResult> GetMediaWikiResultsFromHtml(BaseClasses.Linker linker, string searchUrl, string searchTerm, string websiteUrl, string linkName, int slashCount = 2)
        {
            var result = new List<SearchResult>();

            try
            {
                (var success, var document) = linker.LoadDocument(string.Format(searchUrl, searchTerm.UrlEncode()), "", true);

                if (!success)
                {
                    return null;
                }

                var resultNode = document.DocumentNode.SelectSingleNode("//h2[text()[contains(., 'Page title matches')]]/following::ul[1]")
                    ?? document.DocumentNode.SelectSingleNode("//h2/span[text()[contains(., 'Page title matches')]]/following::ul[1]");

                if (resultNode != null)
                {
                    var htmlNodes = resultNode.SelectNodes("./li[contains(@class, 'mw-search-result')]");

                    if (htmlNodes?.Any() ?? false)
                    {
                        foreach (var node in htmlNodes)
                        {
                            var url = node.SelectSingleNode("./div[@class='mw-search-result-heading']/a").GetAttributeValue("href", "");

                            // MediaWiki returns subpages to games in the results, so we simply
                            // count the slashes to filter them out.
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
                Log.Error(ex, $"Error loading data from {linkName} - {searchUrl}");
            }

            return result;
        }

        /// <summary>
        /// Deserializes an XML string into an object
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
        /// Converts a string to a stream.
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
        /// Converts a wildcard pattern to a regular expression.
        /// * is interpreted as zero or more characters, ? is interpreted as exactly one character.
        /// </summary>
        /// <param name="value">Pattern to convert</param>
        /// <returns>The resulting regular expression</returns>
        internal static string WildCardToRegular(string value) => "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
    }
}