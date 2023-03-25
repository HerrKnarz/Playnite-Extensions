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
    internal class ApiCaller
    {
        private static readonly string _baseUrl = "https://en.wikipedia.org/w/rest.php/v1/";
        private static readonly string _imageUrl = "https://en.wikipedia.org/w/api.php?action=query&format=json&formatversion=2&prop=pageimages|pageterms&piprop=original&pilicense=any&titles={0}";
        private static string SearchUrl => _baseUrl + "search/page?q={0}&limit=100";
        private static string PageUrl => _baseUrl + "page/{0}";

        private static WebClient GetWebClient()
        {
            WebClient client = new WebClient();

            Assembly thisAssem = typeof(WikipediaMetadata).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();

            Version version = thisAssemName.Version;

            client.Headers.Add("user-agent", $"Playnite Wikipedia Metadata Addon/{version} (alex@knarzwerk.de)");
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
        internal static WikipediaPage GetGameData(string key)
        {
            WebClient client = GetWebClient();

            string apiUrl = string.Format(PageUrl, key.UrlEncode());

            string jsonResult = client.DownloadString(apiUrl);

            return JsonConvert.DeserializeObject<WikipediaPage>(jsonResult);
        }
        internal static WikipediaImage GetImage(string key)
        {
            WebClient client = GetWebClient();

            string apiUrl = string.Format(_imageUrl, key.UrlEncode());

            string jsonResult = client.DownloadString(apiUrl);

            return JsonConvert.DeserializeObject<WikipediaImage>(jsonResult);
        }
    }
}
