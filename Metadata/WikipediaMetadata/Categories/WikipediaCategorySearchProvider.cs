using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteExtensions.Metadata.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using WikipediaMetadata.Categories.Models;

namespace WikipediaMetadata.Categories;

public class WikipediaCategorySearchProvider(WikipediaApi api) : IBulkPropertyImportDataSource<WikipediaSearchResult>
{
    private readonly ILogger _logger = LogManager.GetLogger();

    public IEnumerable<WikipediaSearchResult> Search(string query, CancellationToken cancellationToken = default)
    {
        return api.Search(query, WikipediaNamespace.Category);
    }

    public GenericItemOption<WikipediaSearchResult> ToGenericItemOption(WikipediaSearchResult item) => new(item) { Name = item.Name };

    IEnumerable<GameDetails> ISearchableDataSourceWithDetails<WikipediaSearchResult, IEnumerable<GameDetails>>.GetDetails(WikipediaSearchResult searchResult, GlobalProgressActionArgs progressArgs, Game searchGame)
    {
        throw new NotImplementedException();
    }

    public CategoryContents GetCategoryContents(string categoryName, CancellationToken cancellationToken)
    {
        var output = new CategoryContents();

        var categoryMembers = api.GetCategoryMembers(categoryName, cancellationToken);
        foreach (var categoryMember in categoryMembers)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

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
}
