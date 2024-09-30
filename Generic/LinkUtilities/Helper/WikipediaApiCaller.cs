using KNARZhelper;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    /// <summary>
    /// Handles all API calls to Wikipedia
    /// </summary>
    internal class WikipediaApiCaller
    {
        private const string _searchUrl = "https://en.wikipedia.org/w/rest.php/v1/search/page?q={0}&limit=100";

        internal static WikipediaSearchResult GetSearchResults(string name)
        {
            var client = GetWebClient();

            var apiUrl = string.Format(_searchUrl, name.UrlEncode());

            var jsonResult = client.DownloadString(apiUrl);

            return JsonConvert.DeserializeObject<WikipediaSearchResult>(jsonResult);
        }

        private static WebClient GetWebClient()
        {
            var client = new WebClient();

            var thisAssembly = typeof(global::LinkUtilities.LinkUtilities).Assembly;
            var thisAssemblyName = thisAssembly.GetName();

            var version = thisAssemblyName.Version;

            client.Headers.Add("user-agent", $"Playnite LinkUtilities Addon/{version} (alex@knarzwerk.de)");
            client.Encoding = Encoding.UTF8;

            return client;
        }
    }
}