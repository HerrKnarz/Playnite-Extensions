using System.Collections.Generic;

namespace WikipediaCategories.Models.API;

public class PageQuery
{
    public NormalizedTitle[] Normalized { get; set; } = [];
    public Redirects[] Redirects { get; set; } = [];
    public Dictionary<string, PageData> Pages { get; set; } = [];
}

public class NormalizedTitle
{
    public string From { get; set; }
    public string To { get; set; }
}

public class Redirects
{
    public string From { get; set; }
    public string To { get; set; }
}

public class PageData
{
    public int PageId { get; set; }
    public int Ns { get; set; }
    public string Title { get; set; }
    public OtherPageDetails[] Categories { get; set; } = [];
    public OtherPageDetails[] Redirects { get; set; } = [];
}

public class OtherPageDetails
{
    public int Ns { get; set; }
    public string Title { get; set; }
}
