using KNARZhelper;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MetadataUtilities.ViewModels
{
    public class SelectDateViewModel : ObservableObject
    {
        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public DateTime Value { get; set; } = DateTime.Now;

        public static bool ShowDialog(ref DateTime value)
        {
            try
            {
                SelectDateViewModel viewModel = new SelectDateViewModel
                {
                    Value = value
                };

                SelectDateView view = new SelectDateView();

                Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesDialogEnterValue"));
                window.Content = view;
                window.DataContext = viewModel;

                if (!(window.ShowDialog() ?? false))
                {
                    return false;
                }

                value = viewModel.Value;

                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing select date dialog", true);

                return false;
            }
        }
    }
}