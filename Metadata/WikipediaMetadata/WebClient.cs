using ComposableAsync;
using RateLimiter;
using System;
using System.Net.Http;

namespace WikipediaMetadata;

public interface IWebClient
{
    string DownloadString(string url);
}

public class HttpClientWrapper : IWebClient
{
    private readonly HttpClient _httpClient;

    public HttpClientWrapper()
    {
        var handler = TimeLimiter
                      .GetFromMaxCountByInterval(100, TimeSpan.FromMinutes(1))
                      .AsDelegatingHandler();
        _httpClient = new HttpClient(handler);
        var version = typeof(WikipediaMetadata).Assembly.GetName().Version;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"Playnite Wikipedia Metadata Addon/{version} (alex@knarzwerk.de)");
    }

    public string DownloadString(string url)
    {
        return _httpClient.GetStringAsync(url).Result;
    }
}
