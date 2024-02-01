using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace MetadataUtilities
{
    public class SelectMetadataViewModel : ViewModelBase
    {
        private ICollectionView _filteredMetadata;
        private bool _filterSelected;
        private MetadataListObjects _metadataListObjects;
        private MetadataUtilities _plugin;
        private string _searchTerm = string.Empty;

        public ICollectionView FilteredMetadata
        {
            get => _filteredMetadata;
            set
            {
                _filteredMetadata = value;
                OnPropertyChanged("FilteredMetadata");
            }
        }

        public bool FilterSelected
        {
            get => _filterSelected;
            set
            {
                _filterSelected = value;
                FilteredMetadata.Refresh();
                OnPropertyChanged("FilterSelected");
            }
        }

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set
            {
                _plugin = value;
                OnPropertyChanged("Plugin");
            }
        }

        public MetadataListObjects MetadataListObjects
        {
            get => _metadataListObjects;
            set
            {
                _metadataListObjects = value;
                OnPropertyChanged("MetadataListObjects");
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                FilteredMetadata.Refresh();
                OnPropertyChanged("SearchTerm");
            }
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public RelayCommand<Window> CancelCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = false;
            win.Close();
        }, win => win != null);

        public void InitializeView(MetadataUtilities plugin, MetadataListObjects metadataListObjects)
        {
            _plugin = plugin;
            MetadataListObjects = metadataListObjects;

            FilteredMetadata = CollectionViewSource.GetDefaultView(_metadataListObjects);

            FilteredMetadata.Filter = Filter;
        }

        private bool Filter(object item)
        {
            MetadataListObject metadataListObject = item as MetadataListObject;

            return metadataListObject.Name.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase) && (!FilterSelected || metadataListObject.Selected);
        }
    }
}