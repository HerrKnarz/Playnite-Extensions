﻿using KNARZhelper;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MetadataUtilities.ViewModels
{
    public class SelectIntViewModel : ObservableObject
    {
        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public int Value { get; set; } = 0;

        public static bool ShowDialog(ref int value)
        {
            try
            {
                var viewModel = new SelectIntViewModel
                {
                    Value = value
                };

                var view = new SelectIntView();

                var window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesDialogEnterValue"));
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
                Log.Error(exception, "Error during initializing select int dialog", true);

                return false;
            }
        }
    }
}