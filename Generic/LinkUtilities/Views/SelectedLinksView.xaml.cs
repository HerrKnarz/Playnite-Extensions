using KNARZhelper;
using LinkUtilities.Linker;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LinkUtilities
{
    /// <summary>
    /// Interaction logic for SelectedLinksView.xaml
    /// </summary>
    public partial class SelectedLinksView : UserControl
    {
        public SelectedLinksView(Window window)
        {
            try
            {
                InitializeComponent();

                Window = window;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during initializing SelectedLinksView", true);
            }
        }

        public Window Window { get; }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Window.DialogResult = true;
            Window.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Window.DialogResult = false;
            Window.Close();
        }
    }
}