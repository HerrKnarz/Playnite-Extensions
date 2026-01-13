using System.Collections.Generic;
using System.Collections.ObjectModel;
using WikipediaCategories.Models;

namespace WikipediaCategories.BulkImport;

public class SelectCategoriesViewModel(SelectableCategoryViewModel rootCategory)
{
    public IList<SelectableCategoryViewModel> Items { get; } = [rootCategory];
}

public class SelectableCategoryViewModel(string title, CategoryContents contents) : ObservableObject
{
    public CategoryContents Contents { get; set; } = contents;
    public string Title { get; set; } = title;
    public string DisplayName { get; set; } = title.StripCategoryPrefix() + $" ({contents.ArticleNames.Count} articles)";
    public ObservableCollection<SelectableCategoryViewModel> Subcategories { get; set; } = [];

    public bool IsChecked
    {
        get;
        set
        {
            SetValue(ref field, value);
            if (!value)
                foreach (var subcategory in Subcategories)
                    subcategory.IsChecked = value;
        }
    } = true;
}
