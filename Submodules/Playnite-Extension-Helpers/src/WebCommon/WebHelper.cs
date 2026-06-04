using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace PlayniteExtensionHelpers.WebCommon;

public static partial class WebHelper
{
    /// <summary>
    /// Default agent string to use when loading URLs. This is needed to prevent some websites from
    /// blocking the request because of a missing or unknown agent string.
    /// </summary>
    public static readonly string AgentString =
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";

    public static JsonSerializerOptions DefaultJsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Processes an exception thrown during URL loading and extracts error details and status code
    /// if possible.
    /// </summary>
    /// <param name="urlLoadResult">
    /// The result object to populate with error details and status code.
    /// </param>
    /// <param name="exception">The exception thrown during URL loading.</param>
    /// <param name="url">The URL that was being loaded when the exception occurred.</param>
    public static void CatchError(UrlLoadResult urlLoadResult, Exception exception, string url)
    {
        if (exception is WebException webEx)
        {
            if (webEx.Response != null)
            {
                var response = webEx.Response;
                var dataStream = response.GetResponseStream();

                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    urlLoadResult.ErrorDetails = reader.ReadToEnd();
                    urlLoadResult.StatusCode = ((HttpWebResponse)response).StatusCode;
                    reader.Close();

                    Log.Debug($"Error loading url {url} => status code {urlLoadResult.StatusCode}");
                }
            }
        }
        else if (exception is Exception ex)
        {
            urlLoadResult.ErrorDetails = ex.Message;
            urlLoadResult.StatusCode = HttpStatusCode.BadRequest;
            Log.Error(ex, $"Error loading url {url} => {urlLoadResult.ErrorDetails}");
        }
    }

    /// <summary>
    /// Removes the scheme of a URL and adds a missing trailing slash. Is used to compare URLs with
    /// different schemes
    /// </summary>
    /// <param name="url">URL to clean up</param>
    /// <returns>cleaned up URL</returns>
    public static string CleanUpUrl(string? url)
    {
        if (url.IsNullOrEmpty())
        {
            return string.Empty;
        }

        try
        {
            var uri = new Uri(url);

            var urlWithoutScheme = uri.Host + uri.PathAndQuery + uri.Fragment;

            return !urlWithoutScheme.EndsWith('/') ? urlWithoutScheme + '/' : urlWithoutScheme;
        }
        catch (Exception)
        {
            return url;
        }
    }

    /// <summary>
    /// Gets the content of the title tag from a html page.
    /// </summary>
    /// <param name="htmlSource">html page to parse</param>
    /// <returns>decoded title of the page</returns>
    public static string GetPageTitle(string htmlSource) => WebUtility.HtmlDecode(PageTitleRegex().Match(htmlSource).Groups["Title"].Value.Trim());

    [GeneratedRegex(@"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase, "en-CA")]
    private static partial Regex PageTitleRegex();
}