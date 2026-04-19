using Playnite;
using Playnite.WebViews;
using System.Net;
using System.Text.Json;
using System.Windows.Media;

namespace PlayniteCommon.WebCommon;

public enum DocumentType
{
    /// <summary>
    /// Retrieves the source code of the page. Usually used for scraping data from the HTML.
    /// </summary>
    Source = 0,

    /// <summary>
    /// Retrieves the text of the page. Usually used for fetching API results in JSON or XML format.
    /// </summary>
    Text = 1,

    /// <summary>
    /// Doesn't load the content of the page and returns an empty string. Usually used when only
    /// checking if a page is reachable.
    /// </summary>
    Empty = 2
}

public class WebWorker : IDisposable
{
    private readonly bool _detailedDebug = false;
    private readonly WebViewSettings _webViewSettings;
    private readonly IPlayniteApi? API;
    private TaskCompletionSource<bool> _tcs = new();
    private IWebView? _webView;

    public WebWorker(int id, IPlayniteApi? api)
    {
        Id = id;
        API = api;

        _webViewSettings = new WebViewSettings
        {
            JavaScriptEnabled = true,
            UserAgent = WebHelper.AgentString
        };
    }

    public HashSet<string> AllowedCallbackUrls { get; set; } = [];

    public int Id { get; } = 0;

    public string RequestUrl { get; set; } = string.Empty;

    public UrlLoadResult UrlLoadResult { get; set; } = new UrlLoadResult();

    public void Dispose()
    {
        _webView?.Dispose();
        _webView = null;

        GC.SuppressFinalize(this);
    }

    public async Task<T?> GetJsonFromApiAsync<T>(string apiUrl, string apiName, bool debugMode = false)
    {
        try
        {
            var linkCheckResult = await LoadUrlAsync(apiUrl, DocumentType.Text, debugMode);

            if (linkCheckResult.StatusCode != HttpStatusCode.OK)
            {
                Log.Error(new Exception(linkCheckResult.ErrorDetails), $"Error loading data from {apiName} - {apiUrl} - Status code: {linkCheckResult.StatusCode}");
                return default;
            }

            return JsonSerializer.Deserialize<T>(linkCheckResult.PageText, WebHelper.DefaultJsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error loading data from {apiName} - {apiUrl}");
        }

        return default;
    }

    /// <summary>
    /// Checks if a URL is reachable and returns OK
    /// </summary>
    /// <param name="url">URL to check</param>
    /// <param name="sameUrl">
    /// When true the method only returns true, if the response url didn't change.
    /// </param>
    /// <param name="wrongTitle">
    /// Returns false, if the website has this title. Is used to detect certain redirects.
    /// </param>
    /// <param name="debugMode">When true debug messages will be logged</param>
    /// <param name="checkForContent">Content to check for</param>
    /// <returns>True, if the URL is reachable</returns>
    public async Task<bool> IsUrlOkAsync(string url, bool sameUrl = false, string wrongTitle = "", bool? debugMode = false, string checkForContent = "", HashSet<string>? allowedCallbackUrls = null)
    {
        try
        {
            var linkCheckResult = await LoadUrlAsync(url, DocumentType.Empty, debugMode, checkForContent, allowedCallbackUrls);

            return linkCheckResult.ErrorDetails.Length == 0 && (sameUrl
                       ? linkCheckResult.StatusCode == HttpStatusCode.OK && linkCheckResult.ResponseUrl == url
                       : linkCheckResult.StatusCode == HttpStatusCode.OK) &&
                   (wrongTitle == string.Empty || linkCheckResult.PageTitle != wrongTitle);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error checking url {url}");
            return false;
        }
    }

    public async Task<UrlLoadResult> LoadUrlAsync(string url, DocumentType documentType = DocumentType.Source, bool? debugMode = false, string checkForContent = "", HashSet<string>? allowedCallbackUrls = null, bool waitForCallback = true)
    {
        var ts = DateTime.Now;
        string? pageText;
        UrlLoadResult = new UrlLoadResult();

        debugMode ??= false;

        if (debugMode.Value)
        {
            Log.Debug($"Worker {Id} - url {url}: 1. Started loading url.");
        }

        try
        {
            if (!url.IsValidHttpUrl())
            {
                SetErrorResult("Invalid URL format.", url);
                return UrlLoadResult;
            }

            Reset();

            if (_webView is null)
            {
                SetErrorResult("WebView not initialized.", url);
                return UrlLoadResult;
            }

            RequestUrl = url;
            _tcs = new TaskCompletionSource<bool>();

            if (allowedCallbackUrls != null)
            {
                AllowedCallbackUrls.UnionWith(allowedCallbackUrls.Select(WebHelper.CleanUpUrl));
            }
            else
            {
                AllowedCallbackUrls.Clear();
            }

            await _webView.OpenAsync();

            await _webView.NavigateAndWaitAsync(url, new(0, 0, 30));

            pageText = documentType == DocumentType.Text ? await _webView.GetPageTextAsync() : await _webView.GetPageSourceAsync() ?? string.Empty;

            UrlLoadResult.ResponseUrl = _webView.GetCurrentAddress() ?? string.Empty;

            if (debugMode.Value)
            {
                Log.Debug($"Worker {Id} - url {url}: 3. NavigateAndWait - duration: ({(DateTime.Now - ts).TotalMilliseconds} ms)");
                ts = DateTime.Now;
            }

            if (waitForCallback)
            {
                try
                {
                    await AsyncHelper.TimeoutAfter(_tcs.Task, TimeSpan.FromSeconds(10));
                }
                catch
                {
                    if (debugMode.Value)
                    {
                        Log.Debug($"Worker {Id} - url {url}: 2. ResourceLoadedCallback - timeout!");

                        // TODO: Remove notification once I tested it with enough games!
                        var notificattionMessage = new NotificationMessage("LinkUtilities",
                            $"ResourceLoadedCallback timeout{Environment.NewLine}Checked url: {url} {Environment.NewLine}Response url: {UrlLoadResult.ResponseUrl}",
                            NotificationSeverity.Info);

                        API?.Notifications.Add(notificattionMessage);
                    }
                }
            }

            _webViewSettings.ResourceLoadedCallback = null;
            //_webView.Close();

            if (UrlLoadResult.StatusCode == HttpStatusCode.Unused && UrlLoadResult.ResponseUrl.Length != 0)
            {
                UrlLoadResult.StatusCode = HttpStatusCode.SeeOther;
            }

            if (debugMode.Value)
            {
                Log.Debug($"Worker {Id} - url {url}: 4. Callback: / status code {UrlLoadResult.StatusCode} / Request url: {UrlLoadResult.RequestUrl}");
                ts = DateTime.Now;
            }

            if (pageText != null)
            {
                UrlLoadResult.PageTitle = WebHelper.GetPageTitle(pageText);

                if (documentType != DocumentType.Empty)
                {
                    UrlLoadResult.PageText = pageText;
                }
            }

            if (UrlLoadResult.StatusCode != HttpStatusCode.OK)
            {
                return UrlLoadResult;
            }

            if (checkForContent.Length > 0)
            {
                UrlLoadResult.StatusCode = pageText?.Contains(checkForContent) ?? false ? HttpStatusCode.OK : HttpStatusCode.ExpectationFailed;

                if (UrlLoadResult.StatusCode != HttpStatusCode.OK)
                {
                    return UrlLoadResult;
                }
            }

            return UrlLoadResult;
        }
        catch (Exception ex)
        {
            WebHelper.CatchError(UrlLoadResult, ex, url);

            return UrlLoadResult;
        }
        finally
        {
            AllowedCallbackUrls.Clear();

            if (debugMode.Value)
            {
                Log.Debug($"Worker {Id} - url {url}: 5. Finished loading - status code {UrlLoadResult.StatusCode} / duration: ({(DateTime.Now - ts).TotalMilliseconds} ms)  / response url: {UrlLoadResult.ResponseUrl} / title: {UrlLoadResult.PageTitle}.");
            }
        }
    }

    internal void Reset()
    {
        //_webView?.Close();
        _webView?.Dispose();

        _webViewSettings.ResourceLoadedCallback = WebViewCallback;
        _webView = API?.WebView.CreateOffscreenView(_webViewSettings);
    }

    private void SetErrorResult(string errorDetails, string url)
    {
        UrlLoadResult.StatusCode = HttpStatusCode.BadRequest;
        UrlLoadResult.ErrorDetails = errorDetails;
        UrlLoadResult.PageTitle = errorDetails;
        UrlLoadResult.PageText = string.Empty;
        UrlLoadResult.ResponseUrl = string.Empty;
        UrlLoadResult.RequestUrl = url;
        UrlLoadResult.IsUrlValid = false;
    }

    private void WebViewCallback(WebViewResourceLoadedCallback callback)
    {
        try
        {
            if (_detailedDebug)
            {
                Log.Debug($"Worker {Id} - url {RequestUrl}: 2AAA. ResourceLoadedCallback - callback url: {callback.Request.Url} / status code: {(HttpStatusCode)callback.Response.StatusCode}");
            }

            if (WebHelper.CleanUpUrl(RequestUrl) == WebHelper.CleanUpUrl(callback.Request.Url) || AllowedCallbackUrls.Contains(WebHelper.CleanUpUrl(callback.Request.Url)))
            {
                try
                {
                    UrlLoadResult.StatusCode = (HttpStatusCode)callback.Response.StatusCode;
                    UrlLoadResult.RequestUrl = callback.Request.Url ?? string.Empty;
                    Log.Debug($"Worker {Id} - url {RequestUrl}: 2. ResourceLoadedCallback - callback url: {callback.Request.Url} / status code: {UrlLoadResult.StatusCode}");
                    _tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    UrlLoadResult.StatusCode = HttpStatusCode.Unused;
                    UrlLoadResult.ErrorDetails = ex.Message;
                    _tcs.TrySetResult(false);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Worker {Id} - url {RequestUrl}: 2. ResourceLoadedCallback - error!");
        }
    }
}

/// <summary>
/// Contains all relevant information about the result of loading a URL, including the page title,
/// text, status code, and any error details if the loading failed.
/// </summary>
public class UrlLoadResult
{
    public string ErrorDetails { get; set; } = string.Empty;
    public bool IsUrlValid { get; set; } = true;
    public string PageText { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public string RequestUrl { get; set; } = string.Empty;
    public string ResponseUrl { get; set; } = string.Empty;
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.Unused;

    /// <summary>
    /// color brush used to visualize the status of the loaded URL. Green for successful loads
    /// (status code 2xx), yellow for redirects (status code 3xx), and red for client or server
    /// errors (status code 4xx or 5xx). Gray is used as a fallback color.
    /// </summary>
    // TODO: Check if the resources are still the same once themes are implemented and adjust the brush names if needed
    public SolidColorBrush StatusColor => StatusCode is >= HttpStatusCode.OK and < HttpStatusCode.Ambiguous
                ? (SolidColorBrush?)System.Windows.Application.Current?.TryFindResource("PositiveRatingBrush") ?? new SolidColorBrush(Colors.Green)
                : StatusCode is >= HttpStatusCode.Ambiguous and < HttpStatusCode.BadRequest
                    ? (SolidColorBrush?)System.Windows.Application.Current?.TryFindResource("MixedRatingBrush") ?? new SolidColorBrush(Colors.Yellow)
                    : (SolidColorBrush?)System.Windows.Application.Current?.TryFindResource("NegativeRatingBrush") ?? new SolidColorBrush(Colors.Red);
}