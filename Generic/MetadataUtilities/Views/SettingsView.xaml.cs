using System.Windows;
using System.Windows.Controls;

namespace MetadataUtilities.Views
{
    public partial class SettingsView
    {
        public SettingsView() => InitializeComponent();

        private void ButtonContextClick(object sender, RoutedEventArgs e)
        {
            var contextMenu = ((Button)sender)?.ContextMenu;

            if (contextMenu == null)
                return;

            contextMenu.DataContext = ((Button)sender).DataContext;
            contextMenu.IsOpen = true;
        }

        private void ClearSearchBox(object sender, RoutedEventArgs e) => SearchBox.Clear();

        private void LvQuickAdd_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lv = (ListView)sender;

            lv.ScrollIntoView(lv.SelectedItem);
        }
    }
}