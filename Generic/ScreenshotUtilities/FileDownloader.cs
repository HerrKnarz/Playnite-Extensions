using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ScreenshotUtilities
{
    public class FileDownloader : IDisposable
    {
        private bool _disposed;
        private HttpClient _httpClient;
        private static FileDownloader _instance;

        public FileDownloader(HttpClient httpClient = null)
        {
            InitHttpClient(httpClient);
        }

        private void InitHttpClient(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();

            _httpClient.Timeout = TimeSpan.FromSeconds(60);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
        }

        /// <summary>
        /// Downloads a file asynchronously from the <paramref name="uri"/> and places it in the specified <paramref name="directoryPath"/> with the specified <paramref name="fileName"/>.
        /// </summary>
        /// <param name="directoryPath">The relative or absolute path to the directory including the filename to be used.</param>
        /// <param name="uri">The URI for the file to download.</param>
        public async Task<FileInfo> DownloadFileAsync(string directoryPath, Uri uri)
        {
            if (_disposed)
            {
                InitHttpClient();
            }

            File.WriteAllBytes(directoryPath, await _httpClient.GetByteArrayAsync(uri));

            return new FileInfo(directoryPath);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _httpClient.Dispose();
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        public static FileDownloader Instance() => _instance ?? (_instance = new FileDownloader());
    }
}
