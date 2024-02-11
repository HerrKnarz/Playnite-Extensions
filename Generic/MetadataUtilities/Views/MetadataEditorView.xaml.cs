using KNARZhelper;
using System;
using System.Windows;

namespace MetadataUtilities
{
    /// <summary>
    ///     Interaction logic for MetadataEditor.xaml
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

        private void ClearSearchBox(object sender, RoutedEventArgs e) => txtSearchBox.Clear();

        /*  private void DataGridCell_Selected(object sender, RoutedEventArgs e)
          {
              // Lookup for the source to be DataGridCell
              if (e.OriginalSource.GetType() != typeof(DataGridCell))
              {
                  return;
              }

              // Starts the Edit on the row;
              DataGrid grd = (DataGrid)sender;
              grd.BeginEdit(e);
          }

          private void OnCellLostFocus(object sender, RoutedEventArgs e)
          {
              var myCell = sender as DataGridCell;

              if (myCell.IsEditing)
              {

              }
          }*/
    }
}