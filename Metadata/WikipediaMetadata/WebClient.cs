using ComposableAsync;
using RateLimiter;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace WikipediaMetadata;

public interface IWebClient
{
    string DownloadString(string url, CancellationToken cancellationToken = default);

    Task<string> DownloadStringAsync(string url, CancellationToken cancellationToken = default);
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

        var version = Assembly.GetExecutingAssembly().GetName().Version;

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"Playnite Wikipedia Metadata Addon/{version} (alex@knarzwerk.de)");
    }

    public string DownloadString(string url, CancellationToken cancellationToken) => DownloadStringAsync(url, cancellationToken).Result;

    public async Task<string> DownloadStringAsync(string url, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync();
        return content;
    }
}
