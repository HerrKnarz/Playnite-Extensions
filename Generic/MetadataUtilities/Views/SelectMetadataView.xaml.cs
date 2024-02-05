using KNARZhelper;
using System;
using System.Windows;

namespace MetadataUtilities.Views
{
    /// <summary>
    ///     Interaction logic for SelectMetadataView.xaml
    /// </summary>
    public partial class SelectMetadataView
    {
        public SelectMetadataView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Select Metadata dialog", true);
            }
        }

        private void ClearSearchBox(object sender, RoutedEventArgs e) => txtSearchBox.Clear();
    }
}