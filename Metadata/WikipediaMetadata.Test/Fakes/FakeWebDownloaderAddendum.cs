using WikipediaMetadata;

// ReSharper disable once CheckNamespace
namespace PlayniteExtensions.Tests.Common;

public partial class FakeWebDownloader : IWebClient
{
    string IWebClient.DownloadString(string url)
    {
        return DownloadString(url, getContent: true)?.ResponseContent;
    }
}
