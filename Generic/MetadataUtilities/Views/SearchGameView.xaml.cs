using KNARZhelper;
using System;
using System.Windows;

namespace MetadataUtilities.Views
{
    /// <summary>
    /// Interaction logic for SearchGameView.xaml
    /// </summary>
    public partial class SearchGameView
    {
        public SearchGameView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Game Search", true);
            }
        }

        private void ClearSearchBox(object sender, RoutedEventArgs e) => SearchBox.Clear();
    }
}