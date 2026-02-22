using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteExtensions.Common;
using PlayniteExtensions.Metadata.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WikipediaMetadata.Models;
using CategorySearchResult = WikipediaMetadata.Categories.Models.CategorySearchResult;

namespace WikipediaMetadata.Categories;

public class WikipediaCategoryBulkImport : BulkGamePropertyAssigner<CategorySearchResult, GamePropertyImportViewModel>
{
    private readonly WikipediaBulkImportUserInterface _ui;
    private readonly WikipediaCategorySearchProvider _wikipediaDataSource;
    private readonly string _categoryPrefix;

    public WikipediaCategoryBulkImport(PluginSettings settings, IGameDatabaseAPI db, WikipediaBulkImportUserInterface ui, WikipediaCategorySearchProvider dataSource, IPlatformUtility platformUtility, int maxDegreeOfParallelism = 8)
        : base(db, ui, dataSource, platformUtility, new WikipediaIdUtility(), ExternalDatabase.Wikipedia, maxDegreeOfParallelism)
    {
        _ui = ui;
        _wikipediaDataSource = dataSource;
        Ui.AllowEmptySearchQuery = false;
        Ui.DefaultSearch = "Video games";
        _categoryPrefix = settings.TagSettings.FirstOrDefault(c => c.Name == "Categories")?.Prefix;
    }

    public override string MetadataProviderName => "Wikipedia";

    protected override PropertyImportSetting GetPropertyImportSetting(CategorySearchResult searchItem, out string name)
    {
        name = searchItem?.Name.StripCategoryPrefix();
        return new() { ImportTarget = PropertyImportTarget.Tags, Prefix = $"{_categoryPrefix} ".TrimStart() };
    }

    private SelectableCategoryViewModel DownloadRootCategory(CategorySearchResult searchResult)
    {
        SelectableCategoryViewModel rootCategory = null;
        Ui.ShowProgress(progressArgs =>
        {
            var progressTextTemplate = ResourceProvider.GetString("LOCWikipediaMetadataCategoryDownloadText");
            int downloadedCategoryCount = 0;
            var downloadedCategories = new HashSet<string>();
            rootCategory = DownloadCategory(searchResult.Name);
            return;

            SelectableCategoryViewModel DownloadCategory(string categoryName)
            {
                var category = new SelectableCategoryViewModel(categoryName, _wikipediaDataSource.GetCategoryContents(categoryName, progressArgs.CancelToken));
                downloadedCategoryCount++;
                if (downloadedCategoryCount % 5 == 0 || downloadedCategoryCount > 70)
                    progressArgs.Text = string.Format(progressTextTemplate, downloadedCategoryCount); //if you're debugging a unit test, you'll get stuck here - temporarily comment out the line

                foreach (string subcategoryName in category.Contents.SubcategoryNames)
                {
                    if (progressArgs.CancelToken.IsCancellationRequested)
                        break;

                    if (!downloadedCategories.Add(subcategoryName)) //Prevent downloading any category twice
                        continue;

                    category.Subcategories.Add(DownloadCategory(subcategoryName));
                }

                return category;
            }
        }, GetGameDownloadProgressOptions(searchResult));
        return rootCategory;
    }

    protected override IEnumerable<GameDetails> GetGames(CategorySearchResult searchResult)
    {
        var rootCategory = DownloadRootCategory(searchResult);
        if (rootCategory == null)
            return null;

        RemoveEmptyChildren(rootCategory);

        var vm = new SelectCategoriesViewModel(rootCategory);

        if (rootCategory.Subcategories.Count > 0)
            vm = _ui.SelectSubcategories(vm);

        if (vm == null)
            return null;

        var articleNames = GetSelectedCategoryArticleMembers(vm.Items.Single()).Distinct().ToList();

        var output = new List<GameDetails>();

        foreach (string articleName in articleNames.Distinct())
        {
            if (!articleName.IsArticleAboutGame(out string displayTitle))
                continue;

            output.Add(new()
            {
                Names = [displayTitle],
                Id = DbId.Wikipedia("en", articleName).Id,
                Url = WikipediaIdUtility.ToWikipediaUrl("en", articleName),
            });
        }

        return output;


        void RemoveEmptyChildren(SelectableCategoryViewModel category)
        {
            foreach (var subcategory in category.Subcategories)
                RemoveEmptyChildren(subcategory);

            category.Subcategories.RemoveAll(c => c.Subcategories.Count == 0 && c.Contents.ArticleNames.Count == 0);
        }

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

    protected override IEnumerable<PotentialLink> GetPotentialLinks(CategorySearchResult searchItem) => [new(MetadataProviderName, g => g.Url, IsAlreadyLinked)];

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

public class WikipediaBulkImportUserInterface(IPlayniteAPI playniteApi) : BulkPropertyUserInterface(playniteApi)
{
    public virtual SelectCategoriesViewModel SelectSubcategories(SelectCategoriesViewModel vm)
    {
        var window = PlayniteApi.Dialogs.CreateWindow(new() { ShowCloseButton = true });
        var view = new SelectCategoriesView(window, vm);
        window.Content = view;
        window.SizeToContent = SizeToContent.WidthAndHeight;
        window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        window.Title = "Select Categories";

        windowSizedDown = false;
        window.SizeChanged += Window_SizeChanged;
        bool? result = window.ShowDialog();
        window.SizeChanged -= Window_SizeChanged;

        return result switch
        {
            true => vm,
            _ => null
        };
    }
}
