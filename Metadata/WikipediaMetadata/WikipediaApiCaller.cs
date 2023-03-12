using KNARZhelper;
using Playnite.SDK.Data;
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
        public static readonly string BaseUrl = "https://en.wikipedia.org/w/rest.php/v1/";
        public static string SearchUrl { get => BaseUrl + "search/page?q={0}&limit=100"; }
        public static string PageUrl { get => BaseUrl + "page/{0}"; }

        public static readonly string ImageUrl = "https://en.wikipedia.org/w/api.php?action=query&format=json&formatversion=2&prop=pageimages|pageterms&piprop=original&pilicense=any&titles={0}";

        public static WebClient GetWebClient()
        {
            WebClient client = new WebClient();

            Assembly thisAssem = typeof(WikipediaMetadata).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();

            Version version = thisAssemName.Version;

            client.Headers.Add("user-agent", $"Playnite Wikipedia Metadata Addon/{version} (alex@knarzwerk.de)");
            client.Encoding = Encoding.UTF8;

            return client;
        }
        public static WikipediaSearchResult GetSearchResults(string name)
        {
            WebClient client = GetWebClient();

            string apiUrl = string.Format(SearchUrl, name.UrlEncode());

            string jsonResult = client.DownloadString(apiUrl);

            return Serialization.FromJson<WikipediaSearchResult>(jsonResult);
        }
        public static WikipediaGameData GetGameData(string key)
        {
            WebClient client = GetWebClient();

            string apiUrl = string.Format(PageUrl, key.UrlEncode());

            string jsonResult = client.DownloadString(apiUrl);

            return Serialization.FromJson<WikipediaGameData>(jsonResult);
        }
        public static WikipediaImage GetImage(string key)
        {
            WebClient client = GetWebClient();

            string apiUrl = string.Format(ImageUrl, key.UrlEncode());

            string jsonResult = client.DownloadString(apiUrl);

            return Serialization.FromJson<WikipediaImage>(jsonResult);
        }
    }
}
