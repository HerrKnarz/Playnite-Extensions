using System.Collections.Generic;

namespace WikipediaCategories.Models;

public class CategoryContents
{
    public List<string> SubcategoryNames { get; } = [];
    public List<string> ArticleNames { get; } = [];
}
