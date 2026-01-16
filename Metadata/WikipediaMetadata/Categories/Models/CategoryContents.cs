using System.Collections.Generic;

namespace WikipediaMetadata.Categories.Models;

public class CategoryContents
{
    public List<string> SubcategoryNames { get; } = [];
    public List<string> ArticleNames { get; } = [];
}
