using KNARZhelper;
using System;
using System.Diagnostics;
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
    }
}