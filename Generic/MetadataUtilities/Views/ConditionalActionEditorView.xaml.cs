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

        private void TextBlockContextClick(object sender, RoutedEventArgs e)
        {
            var contextMenu = ((TextBlock)sender)?.ContextMenu;

            if (contextMenu == null)
            {
                return;
            }

            contextMenu.DataContext = ((TextBlock)sender).DataContext;
            contextMenu.IsOpen = true;
        }
    }
}
