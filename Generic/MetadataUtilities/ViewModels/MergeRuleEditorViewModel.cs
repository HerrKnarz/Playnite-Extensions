using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;

namespace MetadataUtilities
{
    public class MergeRuleEditorViewModel : ObservableObject
    {
        private readonly HashSet<FieldType> _filterTypes = new HashSet<FieldType>();
        private MetadataListObjects _completeMetadata;
        private bool _filterCategories;
        private bool _filterFeatures;
        private bool _filterGenres;
        private bool _filterSelected;
        private bool _filterSeries;
        private bool _filterTags;
        private CollectionViewSource _metadataViewSource;
        private MetadataUtilities _plugin;
        private string _ruleName = string.Empty;
        private FieldType _ruleType = FieldType.Category;
        private string _searchTerm = string.Empty;

        public MergeRuleEditorViewModel(MetadataUtilities plugin, MetadataListObjects objects, HashSet<FieldType> filteredTypes)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Log.Debug("=== MetadataEditorViewModel: Start ===");
                DateTime ts = DateTime.Now;

                Plugin = plugin;
                CompleteMetadata = objects;

                Log.Debug($"=== MetadataEditorViewModel: Start MetadataViewSource ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                ts = DateTime.Now;

                MetadataViewSource = new CollectionViewSource
                {
                    Source = _completeMetadata
                };

                Log.Debug($"=== MetadataEditorViewModel: Source set ({_completeMetadata.Count} rows, {(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                ts = DateTime.Now;

                using (MetadataViewSource.DeferRefresh())
                {
                    FilterCategories = filteredTypes.Contains(FieldType.Category) || !filteredTypes.Any();
                    FilterFeatures = filteredTypes.Contains(FieldType.Feature) || !filteredTypes.Any();
                    FilterGenres = filteredTypes.Contains(FieldType.Genre) || !filteredTypes.Any();
                    FilterSeries = filteredTypes.Contains(FieldType.Series) || !filteredTypes.Any();
                    FilterTags = filteredTypes.Contains(FieldType.Tag) || !filteredTypes.Any();

                    MetadataViewSource.SortDescriptions.Add(new SortDescription("TypeAndName", ListSortDirection.Ascending));
                    MetadataViewSource.IsLiveSortingRequested = true;
                }

                MetadataViewSource.View.Filter = Filter;

                Log.Debug($"=== MetadataEditorViewModel: Filter set ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                ts = DateTime.Now;

                MetadataViewSource.View.MoveCurrentToFirst();

                Log.Debug($"=== MetadataEditorViewModel: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
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

                if (_filterCategories)
                {
                    _filterTypes.Add(FieldType.Category);
                }
                else
                {
                    _filterTypes.Remove(FieldType.Category);
                }

                MetadataViewSource.View.Filter = Filter;
            }
        }

        public bool FilterFeatures
        {
            get => _filterFeatures;
            set
            {
                SetValue(ref _filterFeatures, value);

                if (_filterFeatures)
                {
                    _filterTypes.Add(FieldType.Feature);
                }
                else
                {
                    _filterTypes.Remove(FieldType.Feature);
                }

                MetadataViewSource.View.Filter = Filter;
            }
        }

        public bool FilterGenres
        {
            get => _filterGenres;
            set
            {
                SetValue(ref _filterGenres, value);

                if (_filterGenres)
                {
                    _filterTypes.Add(FieldType.Genre);
                }
                else
                {
                    _filterTypes.Remove(FieldType.Genre);
                }

                MetadataViewSource.View.Filter = Filter;
            }
        }

        public bool FilterSelected
        {
            get => _filterSelected;
            set
            {
                SetValue(ref _filterSelected, value);
                MetadataViewSource.View.Filter = Filter;
            }
        }

        public bool FilterSeries
        {
            get => _filterSeries;
            set
            {
                SetValue(ref _filterSeries, value);

                if (_filterSeries)
                {
                    _filterTypes.Add(FieldType.Series);
                }
                else
                {
                    _filterTypes.Remove(FieldType.Series);
                }

                MetadataViewSource.View.Filter = Filter;
            }
        }

        public bool FilterTags
        {
            get => _filterTags;
            set
            {
                SetValue(ref _filterTags, value);

                if (_filterTags)
                {
                    _filterTypes.Add(FieldType.Tag);
                }
                else
                {
                    _filterTypes.Remove(FieldType.Tag);
                }

                MetadataViewSource.View.Filter = Filter;
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
                MetadataViewSource.View.Filter = Filter;
            }
        }

        public RelayCommand AddNewCommand => new RelayCommand(() =>
        {
            try
            {
                MetadataListObject newItem = new MetadataListObject();

                if (MetadataViewSource.View.CurrentItem != null)
                {
                    newItem.Type = ((MetadataListObject)MetadataViewSource.View.CurrentItem).Type;
                }

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

                    Cursor.Current = Cursors.WaitCursor;

                    newItem.Selected = true;
                    newItem.EditName = newItem.Name;
                    CompleteMetadata.Add(newItem);

                    MetadataViewSource.View.Filter = Filter;
                    MetadataViewSource.View.MoveCurrentTo(newItem);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing merge dialog", true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
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

            return metadataListObject.EditName.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase) &&
                   _filterTypes.Contains(metadataListObject.Type) &&
                   (!FilterSelected || metadataListObject.Selected);
        }
    }
}