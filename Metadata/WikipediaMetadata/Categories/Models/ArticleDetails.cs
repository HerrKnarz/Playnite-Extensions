using System.Collections.Generic;

namespace WikipediaMetadata.Categories.Models;

public class ArticleDetails
{
    public string Title { get; set; }
    public List<string> Categories { get; set; } = [];
    public string Url { get; set; }
}
