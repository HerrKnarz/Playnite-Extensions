﻿using KNARZhelper;
using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.Enum;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace MetadataUtilities.ViewModels
{
    public class AddNewObjectViewModel : ObservableObject
    {
        private bool _enableTypeSelection = true;
        private MetadataObject _newObject;

        public AddNewObjectViewModel(MetadataObject newObject, bool enableTypeSelection = true)
        {
            NewObject = newObject;
            EnableTypeSelection = enableTypeSelection;
            Prefixes.Add(string.Empty);
            Prefixes.AddMissing(ControlCenter.Instance.Settings.Prefixes);
        }

        public bool EnableTypeSelection
        {
            get => _enableTypeSelection;
            set => SetValue(ref _enableTypeSelection, value);
        }

        public Dictionary<FieldType, string> FieldValuePairs => FieldTypeHelper.ItemListFieldValues();

        public MetadataObject NewObject
        {
            get => _newObject;
            set => SetValue(ref _newObject, value);
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = NewObject.Name?.Length != 0;
            win.Close();
        }, win => win != null);

        public ObservableCollection<string> Prefixes { get; } = new ObservableCollection<string>();

        public Visibility PrefixVisibility => ControlCenter.Instance.Settings.Prefixes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public static Window GetWindow(MetadataObject newObject, bool enableTypeSelection = true, string caption = "")
        {
            try
            {
                var viewModel = new AddNewObjectViewModel(newObject, enableTypeSelection);

                var newObjectViewView = new AddNewObjectView();

                if (caption == string.Empty)
                {
                    caption = ResourceProvider.GetString("LOCMetadataUtilitiesDialogAddNewObject");
                }

                var window = WindowHelper.CreateFixedDialog(caption);
                window.Content = newObjectViewView;
                window.DataContext = viewModel;

                return window;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing add new item dialog", true);

                return null;
            }
        }
    }
}