using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteExtensions.Metadata.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using WikipediaCategories.Models;

namespace WikipediaCategories.BulkImport;

public interface IWikipediaCategorySearchProvider : IBulkPropertyImportDataSource<WikipediaSearchResult>
{

}

public class WikipediaCategorySearchProvider(WikipediaApi api) : IWikipediaCategorySearchProvider
{
    public WikipediaApi Api { get; } = api;
    private readonly ILogger _logger = LogManager.GetLogger();

    public IEnumerable<WikipediaSearchResult> Search(string query, CancellationToken cancellationToken = default)
    {
        return Api.Search(query, WikipediaNamespace.Category, cancellationToken);
    }

    public GenericItemOption<WikipediaSearchResult> ToGenericItemOption(WikipediaSearchResult item) => new(item) { Name = item.Name };

    IEnumerable<GameDetails> ISearchableDataSourceWithDetails<WikipediaSearchResult, IEnumerable<GameDetails>>.GetDetails(WikipediaSearchResult searchResult, GlobalProgressActionArgs progressArgs, Game searchGame)
    {
        throw new NotImplementedException();
    }

    public CategoryContents GetCategoryContents(string categoryName, CancellationToken cancellationToken)
    {
        var output = new CategoryContents();

        var categoryMembers = Api.GetCategoryMembers(categoryName, cancellationToken);
        foreach (var categoryMember in categoryMembers)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            switch ((WikipediaNamespace)categoryMember.ns)
            {
                case WikipediaNamespace.Article:
                    output.ArticleNames.Add(categoryMember.title);
                    break;
                case WikipediaNamespace.Category:
                    output.SubcategoryNames.Add(categoryMember.title);
                    break;
                default:
                    _logger.Info($"Unknown wikipedia namespace: {categoryMember.ns} ({categoryMember.title})");
                    break;
            }
        }
        return output;
    }
}
