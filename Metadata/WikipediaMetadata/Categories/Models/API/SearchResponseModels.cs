using System.Collections.Generic;

namespace WikipediaMetadata.Categories.Models.API;

public class SearchQueryResponse
{
    public SearchInfo SearchInfo { get; set; }
    public List<SearchResult> Search { get; set; }
}

public class SearchInfo
{
    public int TotalHits { get; set; }
    public string Suggestion { get; set; }
    public string SuggestionSnippet { get; set; }
}

public class SearchResult
{
    public int ns { get; set; }
    public string title { get; set; }
    public int pageid { get; set; }
    public int size { get; set; }
    public int wordcount { get; set; }
    public string snippet { get; set; }
    public string timestamp { get; set; }
}

