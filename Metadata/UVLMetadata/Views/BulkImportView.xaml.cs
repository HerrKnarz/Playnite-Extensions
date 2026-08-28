using KNARZhelper.Controls;
using System.Windows;
using System.Windows.Controls;

namespace UVLMetadata.Views
{
    /// <summary>
    /// Interaction logic for BulkImportView.xaml
    /// </summary>
    public partial class BulkImportView : UserControl
    {
        //NEXT: Find out why the focus is not set on the first call when opening the window but only from the second time on.

        public BulkImportView(object viewModel)
        {
            InitializeComponent();

            var requestFocus = viewModel as IRequestFocus;

            requestFocus?.FocusRequested += OnFocusRequested;
        }

        private void ClearSearchBox(object sender, RoutedEventArgs e) => SearchBox.Clear();

        private void OnFocusRequested(object sender, FocusRequestedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedGame":
                    DataGridFocusHelper.SelectRowByIndex(GamesGrid, 0);
                    break;
            }
        }
    }
}
