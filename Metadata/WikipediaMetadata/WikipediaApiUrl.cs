using KNARZhelper;
using System;
using System.Collections.Generic;
using System.Text;
using WikipediaMetadata.Models;

namespace WikipediaMetadata;

public static class WikipediaApiUrl
{
    private const string ApiBaseUrl = "https://en.wikipedia.org/w/api.php?format=json";
    private const string RestBaseUrl = "https://en.wikipedia.org/w/rest.php/v1/";
    //https://en.wikipedia.org/api/rest_v1/page/html/

    public static string GetArticleSearchUrl(string query) => GetUrl($"{RestBaseUrl}search/page?limit=100", new() { { "q", query } });

    public static string GetArticleUrl(string pageName, Dictionary<string, string> continueParams = null)
    {
        return GetUrl(new()
        {
            { "action", "query" },
            { "titles", pageName },
            { "prop", "categories|redirects" },
            { "cllimit", "max" },
            { "rdlimit", "max" },
            { "redirects", null },
        }, continueParams);
    }

    public static string GetCategoryMembersUrl(string pageName, Dictionary<string, string> continueParams = null)
    {
        return GetUrl(new()
        {
            { "action", "query" },
            { "list", "categorymembers" },
            { "cmtitle", pageName },
            { "cmlimit", "max" },
            { "cmprop", "title|type" },
            { "cmnamespace", "0|14" }, // 0=articles, 14=categories - sometimes there would be images in here too, this excludes them
        }, continueParams);
    }

    public static string GetPageDataUrl(string articleName) => $"{RestBaseUrl}page/{articleName}";

    public static string GetPageHtmlUrl(string articleName) => "https://en.wikipedia.org/api/rest_v1/page/html/" + articleName.UrlEncode();

    public static string GetPagePropertiesUrl(string articleName)
    {
        return GetUrl(new()
        {
            { "action", "query" },
            { "prop", "pageimages|pageterms|categories" },
            { "piprop", "original" },
            { "pilicense", "any" },
            { "cllimit", "max" },
            { "formatversion", "2" },
            { "titles", articleName },
        });
    }

    public static string GetSearchUrl(string query, WikipediaNamespace ns)
    {
        return GetUrl(new()
        {
            { "action", "query" },
            { "list", "search" },
            { "srsearch", query },
            { "srlimit", "100" },
            { "srnamespace", ((int)ns).ToString() },
        });
    }

    private static string GetUrl(Dictionary<string, string> parameters, Dictionary<string, string> continueParams = null) => GetUrl(ApiBaseUrl, parameters, continueParams);

    private static string GetUrl(string baseUrl, Dictionary<string, string> parameters, Dictionary<string, string> continueParams = null)
    {
        StringBuilder sb = new(baseUrl);

        AddParameters(parameters);
        AddParameters(continueParams);

        return sb.ToString();

        void AddParameters(Dictionary<string, string> localParameters)
        {
            if (localParameters == null)
            {
                return;
            }

            foreach (var parameter in localParameters)
            {
                sb.Append('&').Append(parameter.Key);

                if (!string.IsNullOrEmpty(parameter.Value))
                {
                    sb.Append('=').Append(Uri.EscapeDataString(parameter.Value));
                }
            }
        }
    }
}
