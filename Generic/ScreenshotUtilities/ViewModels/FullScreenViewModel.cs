﻿using KNARZhelper;
using Playnite.SDK;
using ScreenshotUtilities.Models;
using ScreenshotUtilities.Views;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ScreenshotUtilities.ViewModels
{
    internal class FullScreenViewModel : ObservableObject
    {
        private static ScreenshotUtilities _plugin;
        private ScreenshotGroup _selectedGroup;

        public FullScreenViewModel(ScreenshotUtilities plugin, ScreenshotGroup selectedGroup)
        {
            _plugin = plugin;
            SelectedGroup = selectedGroup;
        }

        public static Window GetWindow(ScreenshotUtilities plugin, ScreenshotGroup selectedGroup)
        {
            try
            {
                var window = WindowHelper.CreateFullScreenWindow();

                window.Content = new FullScreenView(plugin, selectedGroup);

                return window;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing full screen view", true);

                return null;
            }
        }

        public RelayCommand<object> SelectPreviousScreenshotCommand => new RelayCommand<object>(a => SelectedGroup?.SelectPreviousScreenshot());

        public RelayCommand<object> SelectNextScreenshotCommand => new RelayCommand<object>(a => SelectedGroup?.SelectNextScreenshot());

        public ScreenshotGroup SelectedGroup
        {
            get => _selectedGroup;
            set => SetValue(ref _selectedGroup, value);
        }
    }
}
