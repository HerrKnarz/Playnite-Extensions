using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteExtensions.Metadata.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using WikipediaMetadata.Categories.Models;
using WikipediaMetadata.Models;

namespace WikipediaMetadata.Categories;

public class WikipediaCategorySearchProvider(WikipediaApi api) : IBulkPropertyImportDataSource<CategorySearchResult>
{
    private readonly ILogger _logger = LogManager.GetLogger();

    public CategoryContents GetCategoryContents(string categoryName, CancellationToken cancellationToken)
    {
        var output = new CategoryContents();

        var categoryMembers = api.GetCategoryMembers(categoryName, cancellationToken);
        foreach (var categoryMember in categoryMembers)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            switch ((WikipediaNamespace)categoryMember.Ns)
            {
                case WikipediaNamespace.Article:
                    output.ArticleNames.Add(categoryMember.Title);
                    break;

                case WikipediaNamespace.Category:
                    output.SubcategoryNames.Add(categoryMember.Title);
                    break;

                default:
                    _logger.Info($"Unknown wikipedia namespace: {categoryMember.Ns} ({categoryMember.Title})");
                    break;
            }
        }
        return output;
    }

    IEnumerable<GameDetails> ISearchableDataSourceWithDetails<CategorySearchResult, IEnumerable<GameDetails>>.GetDetails(CategorySearchResult searchResult, GlobalProgressActionArgs progressArgs, Game searchGame) =>
        // Because this bulk import process requires an extra step in selecting the categories you
        // want to import in a tree view, this step is not handled here, but in WikipediaCategoryBulkImport
        throw new NotImplementedException();

    public IEnumerable<CategorySearchResult> Search(string query, CancellationToken cancellationToken = default) => api.Search(query, WikipediaNamespace.Category);

    public GenericItemOption<Models.CategorySearchResult> ToGenericItemOption(CategorySearchResult item) => new(item) { Name = item.Name };
}
