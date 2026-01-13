using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteExtensions.Common;
using PlayniteExtensions.Metadata.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace WikipediaCategories.BulkImport;

public class WikipediaCategoryBulkImport : BulkGamePropertyAssigner<WikipediaSearchResult, GamePropertyImportViewModel>
{
    private readonly WikipediaBulkImportUserInterface _ui;
    private readonly WikipediaCategorySearchProvider _wikipediaDataSource;

    public WikipediaCategoryBulkImport(IGameDatabaseAPI db, WikipediaBulkImportUserInterface ui, WikipediaCategorySearchProvider dataSource, IPlatformUtility platformUtility, int maxDegreeOfParallelism = 8)
        : base(db, ui, dataSource, platformUtility, new WikipediaIdUtility(), ExternalDatabase.Wikipedia, maxDegreeOfParallelism)
    {
        _ui = ui;
        _wikipediaDataSource = dataSource;
        Ui.AllowEmptySearchQuery = false;
        Ui.DefaultSearch = "Video games set in";
    }

    public override string MetadataProviderName => "Wikipedia";

    protected override PropertyImportSetting GetPropertyImportSetting(WikipediaSearchResult searchItem, out string name)
    {
        name = searchItem?.Name.StripCategoryPrefix();
        return new() { ImportTarget = PropertyImportTarget.Tags };
    }

    private SelectableCategoryViewModel DownloadRootCategory(WikipediaSearchResult searchResult)
    {
        SelectableCategoryViewModel rootCategory = null;
        Ui.ShowProgress(progressArgs =>
        {
            var downloadedCategories = new HashSet<string>();
            rootCategory = DownloadCategory(searchResult.Name);
            return;

            SelectableCategoryViewModel DownloadCategory(string categoryName)
            {
                var category = new SelectableCategoryViewModel(categoryName, _wikipediaDataSource.GetCategoryContents(categoryName, progressArgs.CancelToken));
                foreach (string subcategoryName in category.Contents.SubcategoryNames)
                {
                    if (progressArgs.CancelToken.IsCancellationRequested)
                        break;

                    if (!downloadedCategories.Add(subcategoryName))
                        continue;

                    category.Subcategories.Add(DownloadCategory(subcategoryName));
                }

                return category;
            }
        }, GetGameDownloadProgressOptions(searchResult));
        return rootCategory;
    }

    protected override IEnumerable<GameDetails> GetGames(WikipediaSearchResult searchResult)
    {
        var rootCategory = DownloadRootCategory(searchResult);
        if (rootCategory == null)
            return [];

        var vm = new SelectCategoriesViewModel(rootCategory);

        if (rootCategory.Subcategories.Count > 0)
            vm = _ui.SelectSubcategories(vm);

        if (vm == null)
            return [];

        var articleNames = GetSelectedCategoryArticleMembers(vm.Items.Single()).Distinct().ToList();

        var output = new List<GameDetails>();

        foreach (string articleName in articleNames.Distinct())
        {
            if (!articleName.IsArticleAboutGame(out string displayTitle))
                continue;

            output.Add(new()
            {
                Names = [displayTitle],
                Id = DbId.Wikipedia(_wikipediaDataSource.Api.WikipediaLocale, articleName).Id,
                Url = WikipediaIdUtility.ToWikipediaUrl(_wikipediaDataSource.Api.WikipediaLocale, articleName),
            });
        }

        return output;


        List<string> GetSelectedCategoryArticleMembers(SelectableCategoryViewModel category)
        {
            List<string> articles = [];
            if (category.IsChecked)
                articles.AddRange(category.Contents.ArticleNames);

            foreach (var subcategory in category.Subcategories)
                articles.AddRange(GetSelectedCategoryArticleMembers(subcategory));

            return articles;
        }
    }

    protected override IEnumerable<PotentialLink> GetPotentialLinks(WikipediaSearchResult searchItem) => [new(MetadataProviderName, g => g.Url, IsAlreadyLinked)];

    private bool IsAlreadyLinked(IEnumerable<Link> links, string url)
    {
        var gameId = DatabaseIdUtility.GetIdFromUrl(url);
        foreach (var link in links)
        {
            var linkId = DatabaseIdUtility.GetIdFromUrl(link?.Url);
            if (linkId == gameId)
                return true;
        }

        return false;
    }
}

public static class WikipediaHelper
{
    private static readonly Regex TitleParenthesesRegex = new(@"^(?<title>.+) \((?<parenContents>.+)\)$", RegexOptions.Compiled);

    extension(string articleName)
    {
        public string StripCategoryPrefix() => articleName?.Split([':'], 2).Last();

        public bool IsArticleAboutGame(out string displayTitle)
        {
            displayTitle = articleName;
            var match = TitleParenthesesRegex.Match(articleName);
            if (match.Success)
            {
                var parenContents = match.Groups["parenContents"].Value;
                if (parenContents.Contains("comic") || parenContents.Contains("soundtrack") || parenContents.Contains("film") || parenContents.Contains("novel"))
                    return false;
            }

            displayTitle = match.Success ? match.Groups["title"].Value : articleName;
            return true;
        }
    }
}

public class WikipediaBulkImportUserInterface(IPlayniteAPI playniteApi) : BulkPropertyUserInterface(playniteApi)
{
    public virtual SelectCategoriesViewModel SelectSubcategories(SelectCategoriesViewModel vm)
    {
        var window = PlayniteApi.Dialogs.CreateWindow(new() { ShowCloseButton = true });
        var view = new SelectCategoriesView(window, vm);
        window.Content = view;
        window.SizeToContent = SizeToContent.WidthAndHeight;
        window.Owner = playniteApi.Dialogs.GetCurrentAppWindow();
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        window.Title = "Select Categories";
        var result = window.ShowDialog();
        return result switch
        {
            true => vm,
            _ => null
        };
    }
}
