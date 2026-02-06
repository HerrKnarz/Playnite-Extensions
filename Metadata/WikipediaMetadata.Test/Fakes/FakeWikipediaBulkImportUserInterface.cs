using Playnite.SDK;
using PlayniteExtensions.Metadata.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WikipediaMetadata.Categories;

namespace WikipediaMetadata.Test.Fakes;

public class FakeWikipediaBulkImportUserInterface(string searchQuery, Predicate<GameCheckboxViewModel> gameApprovalPredicate, Predicate<SelectableCategoryViewModel> selectCategoryPredicate)
    : WikipediaBulkImportUserInterface(null)
{

    public override void ShowProgress(Action<GlobalProgressActionArgs> action, GlobalProgressOptions progressOptions)
    {
        GlobalProgressActionArgs args = new(SynchronizationContext.Current, Dispatcher.CurrentDispatcher, CancellationToken.None);
        Task.Run(() => action(args)).Wait();
    }

    public override GamePropertyImportViewModel SelectGames<TApprovalPromptViewModel>(TApprovalPromptViewModel viewModel)
    {
        foreach (var gameCheckboxViewModel in viewModel.Games)
            gameCheckboxViewModel.IsChecked = gameApprovalPredicate(gameCheckboxViewModel);

        return viewModel;
    }

    public override TItem ChooseItemWithSearch<TItem>(List<GenericItemOption<TItem>> items, Func<string, List<GenericItemOption>> searchFunction, string defaultSearch = null, string caption = null)
    {
        return searchFunction(defaultSearch).Cast<GenericItemOption<TItem>>().First().Item;
    }

    public override TSearchItem SelectGameProperty<TSearchItem>(ISearchableDataSourceWithDetails<TSearchItem, IEnumerable<GameDetails>> dataSource)
    {
        return dataSource.Search(searchQuery).First();
    }

    public override SelectCategoriesViewModel SelectSubcategories(SelectCategoriesViewModel vm)
    {
        foreach (var cat in vm.Items)
            SetSelected(cat);

        return vm;

        void SetSelected(SelectableCategoryViewModel category)
        {
            category.IsChecked = selectCategoryPredicate(category);
            foreach (var subcategory in category.Subcategories)
                SetSelected(subcategory);
        }
    }
}
