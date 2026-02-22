using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace WikipediaMetadata.Test.Fakes;

public class FakeWebClient : IWebClient
{
    public IReadOnlyDictionary<string, string> FilesByUrlReadOnly => FilesByUrl;
    private Dictionary<string, string> FilesByUrl { get; } = new(StringComparer.Ordinal);
    public List<string> CalledUrls { get; } = [];

    public FakeWebClient(string url, string file) => AddFile(url, file);

    public FakeWebClient(Dictionary<string, string> urls)
    {
        foreach (var url in urls)
            AddFile(url.Key, url.Value);
    }

    public void AddFile(string url, string file)
    {
        if (!File.Exists(file))
            throw new FileNotFoundException($"File not found: {file}, for URL: {url}");

        FilesByUrl.Add(url, file);
    }

    public string DownloadString(string url, CancellationToken cancellationToken = default)
    {
        CalledUrls.Add(url);
        return FilesByUrl.TryGetValue(url, out string filePath)
            ? File.ReadAllText(filePath)
            : throw new($"Url not accounted for: {url}");
    }

    public Task<string> DownloadStringAsync(string url, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DownloadString(url, cancellationToken));
    }

    public void AssertAllUrlsCalledOnce()
    {
        Assert.Equal(FilesByUrl.Count, CalledUrls.Count);
        Assert.All(FilesByUrl.Keys, url => Assert.Contains(url, CalledUrls));
    }
}
