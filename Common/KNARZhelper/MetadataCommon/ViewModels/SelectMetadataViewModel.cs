using KNARZhelper.MetadataCommon.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace KNARZhelper.MetadataCommon.ViewModels
{
    /// <summary>
    /// View model for selecting metadata objects from a list with filtering capabilities.
    /// </summary>
    public class SelectMetadataViewModel : ObservableObject
    {
        private ICollectionView _filteredMetadata;
        private bool _filterSelected;
        private string _searchTerm = string.Empty;

        /// <summary>
        /// Creates a new instance of the SelectMetadataViewModel class.
        /// </summary>
        /// <param name="items">Collection of metadata objects to select from</param>
        public SelectMetadataViewModel(ObservableCollection<BaseMetadataObject> items)
        {
            FilteredMetadata = CollectionViewSource.GetDefaultView(items);
            FilteredMetadata.Filter = Filter;
        }

        /// <summary>
        /// Gets or sets the filtered view of the metadata objects.
        /// </summary>
        public ICollectionView FilteredMetadata
        {
            get => _filteredMetadata;
            set => SetValue(ref _filteredMetadata, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to filter the list to show only selected metadata objects.
        /// </summary>
        public bool FilterSelected
        {
            get => _filterSelected;
            set
            {
                SetValue(ref _filterSelected, value);
                FilteredMetadata.Refresh();
            }
        }

        /// <summary>
        /// Command to confirm the selection and close the dialog.
        /// </summary>
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

        /// <summary>
        /// Gets a window containing the SelectMetadataView with the specified items and title.
        /// </summary>
        /// <param name="items">Collection of metadata objects to select from</param>
        /// <param name="windowTitle">Title of the window</param>
        /// <returns>Window containing the SelectMetadataView</returns>
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

        /// <summary>
        /// Filter function for the metadata objects based on the search term and selection filter.
        /// </summary>
        /// <param name="item">Metadata object to filter</param>
        /// <returns>True if the item matches the filter criteria, otherwise false</returns>
        private bool Filter(object item) =>
            item is BaseMetadataObject metadataListObject &&
            metadataListObject.Name.RegExIsMatch(SearchTerm) &&
            (!FilterSelected || metadataListObject.Selected);
    }
}