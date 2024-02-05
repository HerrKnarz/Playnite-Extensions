using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MetadataUtilities
{
    public class MergeRuleEditorViewModel : ObservableObject
    {
        private MetadataListObjects _completeMetadata;
        private bool _filterCategories = true;
        private bool _filterFeatures = true;
        private bool _filterGenres = true;
        private bool _filterSelected;
        private bool _filterSeries = true;
        private bool _filterTags = true;
        private CollectionViewSource _metadataViewSource;
        private MetadataUtilities _plugin;
        private string _ruleName = string.Empty;
        private FieldType _ruleType = FieldType.Category;
        private string _searchTerm = string.Empty;

        public MergeRuleEditorViewModel(MetadataUtilities plugin, MetadataListObjects objects)
        {
            Plugin = plugin;
            CompleteMetadata = objects;

            MetadataViewSource = new CollectionViewSource
            {
                Source = _completeMetadata
            };

            MetadataViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            MetadataViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            MetadataViewSource.IsLiveSortingRequested = true;
            MetadataViewSource.View.Filter = Filter;
            MetadataViewSource.View.MoveCurrentToFirst();
            MetadataViewSource.View.Refresh();
        }

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set => SetValue(ref _plugin, value);
        }

        public bool FilterCategories
        {
            get => _filterCategories;
            set
            {
                SetValue(ref _filterCategories, value);
                MetadataViewSource.View.Refresh();
            }
        }

        public bool FilterFeatures
        {
            get => _filterFeatures;
            set
            {
                SetValue(ref _filterFeatures, value);
                MetadataViewSource.View.Refresh();
            }
        }

        public bool FilterGenres
        {
            get => _filterGenres;
            set
            {
                SetValue(ref _filterGenres, value);
                MetadataViewSource.View.Refresh();
            }
        }

        public bool FilterSelected
        {
            get => _filterSelected;
            set
            {
                SetValue(ref _filterSelected, value);
                MetadataViewSource.View.Refresh();
            }
        }

        public bool FilterSeries
        {
            get => _filterSeries;
            set
            {
                SetValue(ref _filterSeries, value);
                MetadataViewSource.View.Refresh();
            }
        }

        public bool FilterTags
        {
            get => _filterTags;
            set
            {
                SetValue(ref _filterTags, value);
                MetadataViewSource.View.Refresh();
            }
        }

        public string RuleName
        {
            get => _ruleName;
            set => SetValue(ref _ruleName, value);
        }

        public FieldType RuleType
        {
            get => _ruleType;
            set => SetValue(ref _ruleType, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                SetValue(ref _searchTerm, value);
                MetadataViewSource.View.Refresh();
            }
        }

        public RelayCommand AddNewCommand => new RelayCommand(() =>
        {
            try
            {
                MetadataListObject newItem = new MetadataListObject();

                Window window = AddNewObjectViewModel.GetWindow(Plugin, newItem);

                if (window == null)
                {
                    return;
                }

                if (window.ShowDialog() ?? false)
                {
                    if (CompleteMetadata.Any(x => x.Type == newItem.Type && x.EditName == newItem.Name))
                    {
                        return;
                    }

                    newItem.Selected = true;
                    newItem.EditName = newItem.Name;
                    CompleteMetadata.Add(newItem);

                    MetadataViewSource.View.Refresh();
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing merge dialog", true);
            }
        });

        public RelayCommand<Window> SaveCommand => new RelayCommand<Window>(win =>
        {
            if (!RuleName?.Any() ?? false)
            {
                API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogNoTargetSet"), string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!CompleteMetadata.Any(x => x.Selected))
            {
                API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogNoItemsSelected"), string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            win.DialogResult = true;
            win.Close();
        });

        public RelayCommand<object> SetAsTargetCommand => new RelayCommand<object>(item =>
        {
            RuleType = ((MetadataListObject)item).Type;
            RuleName = ((MetadataListObject)item).Name;
        });

        public CollectionViewSource MetadataViewSource
        {
            get => _metadataViewSource;
            set => SetValue(ref _metadataViewSource, value);
        }

        public MetadataListObjects CompleteMetadata
        {
            get => _completeMetadata;
            set => SetValue(ref _completeMetadata, value);
        }

        private bool Filter(object item)
        {
            MetadataListObject metadataListObject = item as MetadataListObject;

            List<FieldType> types = new List<FieldType>();

            if (FilterCategories)
            {
                types.Add(FieldType.Category);
            }

            if (FilterFeatures)
            {
                types.Add(FieldType.Feature);
            }

            if (FilterGenres)
            {
                types.Add(FieldType.Genre);
            }

            if (FilterSeries)
            {
                types.Add(FieldType.Series);
            }

            if (FilterTags)
            {
                types.Add(FieldType.Tag);
            }

            return metadataListObject.EditName.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase) &&
                   types.Any(t => t == metadataListObject.Type) &&
                   (!FilterSelected || metadataListObject.Selected);
        }
    }
}