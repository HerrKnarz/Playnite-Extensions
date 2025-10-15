﻿using KNARZhelper.MetadataCommon.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace KNARZhelper.MetadataCommon.ViewModels
{
    public class SelectMetadataViewModel : ObservableObject
    {
        private ICollectionView _filteredMetadata;
        private bool _filterSelected;
        private string _searchTerm = string.Empty;

        public SelectMetadataViewModel(ObservableCollection<BaseMetadataObject> items)
        {
            FilteredMetadata = CollectionViewSource.GetDefaultView(items);
            FilteredMetadata.Filter = Filter;
        }

        public ICollectionView FilteredMetadata
        {
            get => _filteredMetadata;
            set => SetValue(ref _filteredMetadata, value);
        }

        public bool FilterSelected
        {
            get => _filterSelected;
            set
            {
                SetValue(ref _filterSelected, value);
                FilteredMetadata.Refresh();
            }
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                SetValue(ref _searchTerm, value);
                FilteredMetadata.Refresh();
            }
        }

        public static Window GetWindow(ObservableCollection<BaseMetadataObject> items, string windowTitle)
        {
            try
            {
                var viewModel = new SelectMetadataViewModel(items);

                var selectMetadataView = new SelectMetadataView();

                var window = WindowHelper.CreateFixedDialog(windowTitle);

                window.Content = selectMetadataView;
                window.DataContext = viewModel;

                return window;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing select metadata dialog", true);

                return null;
            }
        }

        private bool Filter(object item) =>
            item is BaseMetadataObject metadataListObject &&
            metadataListObject.Name.RegExIsMatch(SearchTerm) &&
            (!FilterSelected || metadataListObject.Selected);
    }
}