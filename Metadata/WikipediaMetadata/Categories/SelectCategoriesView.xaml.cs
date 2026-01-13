using System.Windows;
using System.Windows.Controls;

namespace WikipediaCategories.BulkImport;

/// <summary>
/// Interaction logic for SelectStringsView.xaml
/// </summary>
public partial class SelectCategoriesView : UserControl
{
    public SelectCategoriesView(Window window, SelectCategoriesViewModel vm)
    {
        InitializeComponent();
        Window = window;
        DataContext = vm;
    }

    public Window Window { get; }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        Window.DialogResult = true;
        Window.Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Window.DialogResult = false;
        Window.Close();
    }

    private void CheckAll_Click(object sender, RoutedEventArgs e) => SetAllChecked(true);

    private void UncheckAll_Click(object sender, RoutedEventArgs e) => SetAllChecked(false);

    private void SetAllChecked(bool isChecked)
    {
        var vm = (SelectCategoriesViewModel)DataContext;
        foreach (var category in vm.Items)
            SetChecked(category);

        return;

        void SetChecked(SelectableCategoryViewModel category)
        {
            category.IsChecked = isChecked;
            foreach (var subcategory in category.Subcategories)
                SetChecked(subcategory);
        }
    }
}
