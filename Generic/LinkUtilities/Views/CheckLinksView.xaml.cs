using KNARZhelper;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

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

        private void HelpClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/HerrKnarz/Playnite-Extensions/wiki/Link-Utilities:-Check-links"));
            e.Handled = true;
        }

        private void UrlClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo((string)((Button)sender).Tag));
            e.Handled = true;
        }
    }
}