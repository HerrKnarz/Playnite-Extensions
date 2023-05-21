using KNARZhelper;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace LinkUtilities
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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void HelpClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/HerrKnarz/Playnite-Extensions/wiki/Link-Utilities:-URL-handler-and-bookmarklet#bookmarklet"));
            e.Handled = true;
        }

        private void UrlClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo((string)((Button)sender).Tag));
            e.Handled = true;
        }
    }
}