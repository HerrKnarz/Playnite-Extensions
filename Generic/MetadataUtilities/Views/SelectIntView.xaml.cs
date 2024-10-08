﻿using KNARZhelper;
using System.Windows;
using System.Windows.Input;

namespace MetadataUtilities.Views
{
    /// <summary>
    /// Interaction logic for PrefixItemControl.xaml
    /// </summary>
    public partial class SelectIntView
    {
        public SelectIntView() => InitializeComponent();

        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!MiscHelper.IsOnlyNumbers(text))
                    e.CancelCommand();
            }
            else
                e.CancelCommand();
        }

        private void PreviewInput(object sender, TextCompositionEventArgs e) => e.Handled = !MiscHelper.IsOnlyNumbers(e.Text);
    }
}