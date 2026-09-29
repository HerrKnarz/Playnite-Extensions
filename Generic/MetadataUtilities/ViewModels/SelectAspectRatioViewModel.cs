using KNARZhelper;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MetadataUtilities.ViewModels
{
    internal class SelectAspectRatioViewModel : ObservableObject
    {
        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public int ValueX { get; set; } = 0;
        public int ValueY { get; set; } = 0;

        public static bool ShowDialog(ref int valueX, ref int valueY)
        {
            try
            {
                var viewModel = new SelectAspectRatioViewModel
                {
                    ValueX = valueX,
                    ValueY = valueY
                };

                var view = new SelectAspectRatioView();

                var window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesDialogEnterValue"));
                window.Content = view;
                window.DataContext = viewModel;

                if (!(window.ShowDialog() ?? false))
                {
                    return false;
                }

                valueX = viewModel.ValueX;
                valueY = viewModel.ValueY;

                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing select aspect ratio dialog", true);

                return false;
            }
        }
    }
}
