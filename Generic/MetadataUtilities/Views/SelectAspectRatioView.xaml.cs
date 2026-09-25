using KNARZhelper;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetadataUtilities.Views
{
    /// <summary>
    /// Interaction logic for SelectAspectRatioView.xaml
    /// </summary>
    public partial class SelectAspectRatioView : UserControl
    {
        public SelectAspectRatioView()
        {
            InitializeComponent();
        }

        private void GotFocusHandler(object sender, RoutedEventArgs e) => Dispatcher.BeginInvoke(new Action(() => ((TextBox)sender).SelectAll()));

        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!MiscHelper.IsOnlyNumbers(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void PreviewInput(object sender, TextCompositionEventArgs e) => e.Handled = !MiscHelper.IsOnlyNumbers(e.Text);
    }
}
