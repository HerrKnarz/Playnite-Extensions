using System.Windows;
using System.Windows.Controls;

namespace UVLMetadata.Views
{
    /// <summary>
    /// Interaction logic for TagCategorySelector.xaml
    /// </summary>
    public partial class BulkImportView : UserControl
    {
        public BulkImportView()
        {
            InitializeComponent();
        }

        private void ClearSearchBox(object sender, RoutedEventArgs e) => SearchBox.Clear();

        private void Button_Click()
        {

        }
    }
}
