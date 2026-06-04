using PlayniteExtensionHelpers.WebCommon;
using System.Text;
using System.Text.Json;

namespace PlayniteExtensionHelpers
{
    public static class ApiHelper
    {
        public static JsonSerializerOptions JsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Gets a JSON result from an API and de-serializes it.
        /// </summary>
        /// <typeparam name="T">Type the JSON gets de-serialized to</typeparam>
        /// <param name="apiUrl">Url to fetch the JSON result from</param>
        /// <param name="apiName">API name for the error message</param>
        /// <param name="encoding">the encoding to use</param>
        /// <param name="body">the body to send to the api</param>
        /// <returns>De-serialized JSON result</returns>
        public static async Task<T?> GetJsonFromApiAsync<T>(IHttpClient? httpClient, string apiUrl, string apiName, Encoding? encoding = null, string? body = null)
        {
            try
            {
                var pageSource = string.Empty;

                encoding ??= Encoding.Default;

                httpClient ??= new HttpClientWrapper(accept: "application/json");

                var uri = new Uri(apiUrl);

                var task = body.IsNullOrEmpty()
                    ? httpClient.DownloadStringAsync(uri.ToString())
                    : httpClient.UploadStringAsync(uri.ToString(), body, "application/json");

                pageSource = await Task.WhenAny(task, Task.Delay(10000)) == task
                    ? task.Result
                    : throw new Exception(
                        $"Timeout loading data from {apiName} - {apiUrl}");

                return JsonSerializer.Deserialize<T>(pageSource, JsonSerializerOptions);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading data from {apiName} - {apiUrl}");
            }

            return default;
        }
    }
}