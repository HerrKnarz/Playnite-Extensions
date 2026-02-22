using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using WikipediaMetadata.Categories.Models;
using WikipediaMetadata.Categories.Models.API;
using WikipediaMetadata.Models;

namespace WikipediaMetadata;

/// <summary>
/// Handles all API calls to Wikipedia
/// </summary>
public class WikipediaApi(IWebClient webClient)
{
    public ICollection<CategoryMember> GetCategoryMembers(string pageName, CancellationToken cancellationToken = default)
    {
        var output = new List<CategoryMember>();
        Dictionary<string, string> continueParams = null;
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var response = GetCategoryMembers(pageName, continueParams, cancellationToken);
            output.AddRange(response.Query.CategoryMembers);

            continueParams = response.Continue;

            if (continueParams == null)
            {
                break;
            }
        }

        return output;
    }

    public WikipediaPage GetGameData(string key) => GetObject<WikipediaPage>(WikipediaApiUrl.GetPageDataUrl(key));

    public string GetPageHtml(string pageName) => webClient.DownloadString(WikipediaApiUrl.GetPageHtmlUrl(pageName));

    public PagePropertiesResponse GetPageProperties(string key) => GetObject<PagePropertiesResponse>(WikipediaApiUrl.GetPagePropertiesUrl(key));

    public WikipediaSearchResult GetSearchResults(string name) => GetObject<WikipediaSearchResult>(WikipediaApiUrl.GetArticleSearchUrl(name));

    public IEnumerable<CategorySearchResult> Search(string query, WikipediaNamespace ns)
    {
        var url = WikipediaApiUrl.GetSearchUrl(query, ns);
        var response = webClient.DownloadString(url);
        var responseObj = JsonConvert.DeserializeObject<WikipediaQueryResponse<SearchQueryResponse>>(response);

        foreach (var searchResult in responseObj.Query.Search)
        {
            yield return new() { Name = searchResult.title };
        }
    }

    private WikipediaQueryResponse<CategoryMemberQueryResult> GetCategoryMembers(string pageName, Dictionary<string, string> continueParams, CancellationToken cancellationToken)
    {
        var url = WikipediaApiUrl.GetCategoryMembersUrl(pageName, continueParams);

        var response = webClient.DownloadString(url, cancellationToken);
        var responseObj = JsonConvert.DeserializeObject<WikipediaQueryResponse<CategoryMemberQueryResult>>(response);
        return responseObj;
    }

    private T GetObject<T>(string url)
    {
        var jsonResult = webClient.DownloadString(url);

        return JsonConvert.DeserializeObject<T>(jsonResult);
    }
}
