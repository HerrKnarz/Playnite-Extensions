using KNARZhelper;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace LinkUtilities.Views
{
    /// <summary>
    ///     Interaction logic for CheckLinksView.xaml
    /// </summary>
    public partial class CheckLinksView : UserControl
    {
        public CheckLinksView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing CheckLinksView", true);
            }
        }

        private void UrlClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo((string)((Button)sender).Tag));
            e.Handled = true;
        }
    }
}