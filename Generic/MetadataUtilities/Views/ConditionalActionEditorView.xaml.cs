using KNARZhelper;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MetadataUtilities.Views
{
    /// <summary>
    /// Interaction logic for ConditionalActionEditorView.xaml
    /// </summary>
    public partial class ConditionalActionEditorView : UserControl
    {
        public ConditionalActionEditorView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing Conditional Action Editor", true);
            }
        }

        private void ButtonContextClick(object sender, RoutedEventArgs e)
        {
            var contextMenu = ((Button)sender)?.ContextMenu;

            if (contextMenu == null)
                return;

            contextMenu.DataContext = ((Button)sender).DataContext;
            contextMenu.IsOpen = true;
        }
    }
}