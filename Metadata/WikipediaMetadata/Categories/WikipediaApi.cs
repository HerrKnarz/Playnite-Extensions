using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Playnite.SDK;
using PlayniteExtensions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WikipediaMetadata.Categories.Models;
using WikipediaMetadata.Categories.Models.API;

namespace WikipediaMetadata.Categories;

public class WikipediaApi
{
    private readonly string _baseUrl;
    private readonly IWebDownloader _downloader;
    private readonly ILogger _logger = LogManager.GetLogger();
    public string WikipediaLocale { get; }

    public WikipediaApi(IWebDownloader downloader, Version playniteVersion, string wikipediaLocale = "en")
    {
        _downloader = downloader;
        WikipediaLocale = wikipediaLocale;
        _baseUrl = $"https://{wikipediaLocale}.wikipedia.org/w/api.php?format=json";

        var pluginVersion = GetType().Assembly!.GetName().Version;
        _downloader.UserAgent = $"Wikipedia plugin {pluginVersion} for Playnite {playniteVersion}";
    }

    public string GetSearchUrl(string query, WikipediaNamespace ns)
    {
        return GetUrl(new()
        {
            { "action", "query" },
            { "list", "search" },
            { "srsearch", query },
            { "limit", "100" },
            { "srnamespace", ((int)ns).ToString() },
        });
    }

    public string GetArticleUrl(string pageName, Dictionary<string, string> continueParams = null)
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

    public string GetCategoryMembersUrl(string pageName, Dictionary<string, string> continueParams = null)
    {
        return GetUrl(new()
        {
            { "action", "query" },
            { "list", "categorymembers" },
            { "cmtitle", pageName },
            { "cmlimit", "max" },
            { "cmprop", "title|type" },
            { "cmnamespace", "0|14" }, // 0=articles, 14=categories - sometimes there would be images in here too
        }, continueParams);
    }

    public IEnumerable<WikipediaSearchResult> Search(string query, WikipediaNamespace ns, CancellationToken cancellationToken = default)
    {
        var url = GetSearchUrl(query, ns);
        var response = _downloader.DownloadString(url, cancellationToken: cancellationToken);
        var responseObj = JsonConvert.DeserializeObject<WikipediaQueryResponse<SearchQueryResponse>>(response.ResponseContent);

        foreach (var searchResult in responseObj.Query.Search)
            yield return new() { Name = searchResult.title };
    }

    public ArticleDetails GetArticleCategories(string pageName, CancellationToken cancellationToken = default)
    {
        var output = new ArticleDetails();
        Dictionary<string, string> continueParams = null;
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var article = GetArticleCategories(pageName, continueParams, cancellationToken);
            var page = article.Query.Pages.First().Value;
            output.Title ??= page.Title;
            output.Url ??= WikipediaIdUtility.ToWikipediaUrl(WikipediaLocale, page.Title);
            output.Categories.AddRange(page.Categories.Select(c => c.Title));

            continueParams = article.Continue;

            if (continueParams == null)
                break;
        }

        return output;
    }

    public ICollection<CategoryMember> GetCategoryMembers(string pageName, CancellationToken cancellationToken = default)
    {
        var output = new List<CategoryMember>();
        Dictionary<string, string> continueParams = null;
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var response = GetCategoryMembers(pageName, continueParams, cancellationToken);
            output.AddRange(response.Query.CategoryMembers);

            continueParams = response.Continue;

            if (continueParams == null)
                break;
        }

        return output;
    }

    private WikipediaQueryResponse<PageQuery> GetArticleCategories(string pageName, Dictionary<string, string> continueParams, CancellationToken cancellationToken)
    {
        var url = GetArticleUrl(pageName, continueParams);

        var response = _downloader.DownloadString(url, cancellationToken: cancellationToken);
        var responseObj = JsonConvert.DeserializeObject<WikipediaQueryResponse<PageQuery>>(response.ResponseContent);
        return responseObj;
    }

    private WikipediaQueryResponse<CategoryMemberQueryResult> GetCategoryMembers(string pageName, Dictionary<string, string> continueParams, CancellationToken cancellationToken)
    {
        var url = GetCategoryMembersUrl(pageName, continueParams);

        var response = _downloader.DownloadString(url, cancellationToken: cancellationToken);
        var responseObj = JsonConvert.DeserializeObject<WikipediaQueryResponse<CategoryMemberQueryResult>>(response.ResponseContent);
        return responseObj;
    }

    private string GetUrl(Dictionary<string, string> parameters, Dictionary<string, string> continueParams = null)
    {
        StringBuilder sb = new(_baseUrl);

        AddParameters(parameters);
        AddParameters(continueParams);

        return sb.ToString();

        void AddParameters(Dictionary<string, string> localParameters)
        {
            if (localParameters == null)
                return;

            foreach (var parameter in localParameters)
            {
                sb.Append('&').Append(parameter.Key);

                if (!string.IsNullOrEmpty(parameter.Value))
                    sb.Append('=').Append(Uri.EscapeDataString(parameter.Value));
            }
        }
    }
}
