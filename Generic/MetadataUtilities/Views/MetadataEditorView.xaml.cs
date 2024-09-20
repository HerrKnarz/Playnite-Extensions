using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MetadataUtilities.Views
{
    /// <summary>
    /// Interaction logic for MetadataEditor.xaml
    /// </summary>
    public partial class MetadataEditorView
    {
        public MetadataEditorView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Metadata Editor", true);
            }
        }

        private void ClearSearchBox(object sender, RoutedEventArgs e) => SearchBox.Clear();

        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingElement is ComboBox t)
            {
                t.IsDropDownOpen = true;
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is DataGrid grid) || (e.OriginalSource as FrameworkElement)?.Name != "MetadataGrid")
            {
                return;
            }

            if (DataContext is MetadataEditorViewModel viewModel)
            {
                viewModel.SelectedItems = grid.SelectedItems.OfType<MetadataObject>().ToList();
            }
        }
    }
}