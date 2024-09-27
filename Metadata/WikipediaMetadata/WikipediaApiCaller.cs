using KNARZhelper;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using WikipediaMetadata.Models;

namespace WikipediaMetadata
{
    /// <summary>
    ///     Handles all API calls to Wikipedia
    /// </summary>
    internal class WikipediaApiCaller
    {
        private const string _baseUrl = "https://en.wikipedia.org/w/rest.php/v1/";
        private const string _imageUrl = "https://en.wikipedia.org/w/api.php?action=query&format=json&formatversion=2&prop=pageimages|pageterms&piprop=original&pilicense=any&titles={0}";
        private static string PageUrl => _baseUrl + "page/{0}";
        private static string SearchUrl => _baseUrl + "search/page?q={0}&limit=100";

        internal static WikipediaPage GetGameData(string key) => GetObject<WikipediaPage>(PageUrl, key);

        internal static WikipediaImage GetImage(string key) => GetObject<WikipediaImage>(_imageUrl, key);

        internal static WikipediaSearchResult GetSearchResults(string name) => GetObject<WikipediaSearchResult>(SearchUrl, name);

        private static T GetObject<T>(string url, string key)
        {
            var apiUrl = string.Format(url, key.UrlEncode());

            var jsonResult = GetWebClient().DownloadString(apiUrl);

            return JsonConvert.DeserializeObject<T>(jsonResult);
        }

        private static WebClient GetWebClient()
        {
            var client = new WebClient();
            var version = typeof(WikipediaMetadata).Assembly.GetName().Version;

            client.Headers.Add("user-agent", $"Playnite Wikipedia Metadata Addon/{version} (alex@knarzwerk.de)");
            client.Encoding = Encoding.UTF8;

            return client;
        }
    }
}