using System;
using System.Windows;
using System.Windows.Controls;
using KNARZhelper;

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

        private void b_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = ((Button)sender)?.ContextMenu;

            if (contextMenu == null)
                return;

            contextMenu.DataContext = ((Button)sender).DataContext;
            contextMenu.PlacementTarget = ((Button)sender);
            contextMenu.IsOpen = true;
        }
    }
}