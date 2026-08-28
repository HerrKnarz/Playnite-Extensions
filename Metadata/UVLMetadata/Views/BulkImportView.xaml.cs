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
                case "MatchedGames":
                    DataGridFocusHelper.SelectRowByIndex(GamesGrid, 0);
                    break;
            }
        }
    }
}
