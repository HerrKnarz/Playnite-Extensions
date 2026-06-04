using System.Net.Http;
using System.Reflection;
using System.Text;

namespace PlayniteExtensionHelpers.WebCommon;

public interface IHttpClient
{
    Task<string> DownloadStringAsync(string url, CancellationToken cancellationToken = default);

    Task<string> UploadStringAsync(string url, string data, string contentType, CancellationToken cancellationToken = default);
}

public class HttpClientWrapper : IHttpClient
{
    private readonly HttpClient _httpClient;

    public HttpClientWrapper(string? userAgent = null, string? accept = null)
    {
        _httpClient = new HttpClient();

        var assembly = Assembly.GetExecutingAssembly().GetName();

        if (string.IsNullOrEmpty(userAgent))
        {
            userAgent = $"Playnite {assembly.Name} Addon/{assembly.Version} (alex@knarzwerk.de)";
        }

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        if (!accept.IsNullOrEmpty())
        {
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd(accept);
        }
    }

    public async Task<string> DownloadStringAsync(string url, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return content;
    }

    public async Task<string> UploadStringAsync(string url, string data, string contentType, CancellationToken cancellationToken)
    {
        //TODO: Untested! Test this as soon as it's needed.
        var content = new StringContent(data, Encoding.UTF8, contentType);
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseContent;
    }
}