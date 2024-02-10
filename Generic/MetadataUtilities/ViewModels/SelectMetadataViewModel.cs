using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace MetadataUtilities
{
    public class SelectMetadataViewModel : ObservableObject
    {
        private ICollectionView _filteredMetadata;
        private bool _filterSelected;
        private MetadataUtilities _plugin;
        private string _searchTerm = string.Empty;

        public SelectMetadataViewModel(MetadataUtilities plugin, MetadataListObjects items)
        {
            Plugin = plugin;
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

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set => SetValue(ref _plugin, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                SetValue(ref _searchTerm, value);
                FilteredMetadata.Refresh();
            }
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public static Window GetWindow(MetadataUtilities plugin, MetadataListObjects items, string windowTitle)
        {
            try
            {
                SelectMetadataViewModel viewModel = new SelectMetadataViewModel(plugin, items);

                SelectMetadataView selectMetadataView = new SelectMetadataView();

                Window window = WindowHelper.CreateFixedDialog(windowTitle);

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
            item is MetadataListObject metadataListObject &&
            metadataListObject.Name.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase) &&
            (!FilterSelected || metadataListObject.Selected);
    }
}