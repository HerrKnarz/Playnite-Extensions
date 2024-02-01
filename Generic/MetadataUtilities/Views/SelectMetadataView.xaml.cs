using KNARZhelper;
using MetadataUtilities.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MetadataUtilities.Views
{
    /// <summary>
    ///     Interaction logic for SelectMetadataView.xaml
    /// </summary>
    public partial class SelectMetadataView : UserControl
    {
        public SelectMetadataView(MetadataUtilities plugin, MetadataListObjects items)
        {
            try
            {
                InitializeComponent();
                ((SelectMetadataViewModel)DataContext).InitializeView(plugin, items);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Select Metadata dialog", true);
            }
        }

        private void ClearSearchBox(object sender, RoutedEventArgs e) => txtSearchBox.Clear();
    }
}