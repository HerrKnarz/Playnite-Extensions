using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace LinkUtilities
{
    public partial class LinkUtilitiesSettingsView : UserControl
    {
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public LinkUtilitiesSettingsView()
        {
            InitializeComponent();
        }
    }
}