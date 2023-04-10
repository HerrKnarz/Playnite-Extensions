using KNARZhelper;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Reflection;
using System.Text;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    /// <summary>
    /// Handles all API calls to Wikipedia
    /// </summary>
    internal class WikipediaApiCaller
    {
        private const string SearchUrl = "https://en.wikipedia.org/w/rest.php/v1/search/page?q={0}&limit=100";

        private static WebClient GetWebClient()
        {
            WebClient client = new WebClient();

            Assembly thisAssembly = typeof(LinkUtilities.LinkUtilities).Assembly;
            AssemblyName thisAssemblyName = thisAssembly.GetName();

            Version version = thisAssemblyName.Version;

            client.Headers.Add("user-agent", $"Playnite LinkUtilities Addon/{version} (alex@knarzwerk.de)");
            client.Encoding = Encoding.UTF8;

            return client;
        }

        internal static WikipediaSearchResult GetSearchResults(string name)
        {
            WebClient client = GetWebClient();

            string apiUrl = string.Format(SearchUrl, name.UrlEncode());

            string jsonResult = client.DownloadString(apiUrl);

            return JsonConvert.DeserializeObject<WikipediaSearchResult>(jsonResult);
        }
    }
}