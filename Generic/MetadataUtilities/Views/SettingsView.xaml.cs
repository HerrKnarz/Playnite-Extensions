using System.Windows;
using System.Windows.Controls;

namespace MetadataUtilities.Views
{
    public partial class SettingsView
    {
        public SettingsView() => InitializeComponent();

        private void b_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = ((Button)sender)?.ContextMenu;

            if (contextMenu == null)
                return;

            contextMenu.DataContext = ((Button)sender).DataContext;
            contextMenu.IsOpen = true;
        }
    }
}