using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;

namespace MetadataUtilities.ViewModels
{
    public class MergeRuleEditorViewModel : ObservableObject
    {
        private readonly HashSet<SettableFieldType> _filterTypes = new HashSet<SettableFieldType>();
        private MetadataObjects _completeMetadata;
        private bool _filterAgeRatings;
        private bool _filterCategories;
        private bool _filterFeatures;
        private bool _filterGenres;
        private string _filterPrefix = string.Empty;
        private bool _filterSelected;
        private bool _filterSeries;
        private bool _filterTags;
        private CollectionViewSource _metadataViewSource;
        private MetadataUtilities _plugin;
        private string _ruleName = string.Empty;
        private SettableFieldType _ruleType = SettableFieldType.Category;
        private string _searchTerm = string.Empty;

        public MergeRuleEditorViewModel(MetadataUtilities plugin, MetadataObjects objects, ICollection<SettableFieldType> filteredTypes)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Log.Debug("=== MetadataEditorViewModel: Start ===");
                DateTime ts = DateTime.Now;

                Plugin = plugin;
                CompleteMetadata = objects;

                Prefixes.Add(string.Empty);
                Prefixes.AddMissing(Plugin.Settings.Settings.Prefixes);

                Log.Debug($"=== MetadataEditorViewModel: Start MetadataViewSource ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                ts = DateTime.Now;

                MetadataViewSource = new CollectionViewSource
                {
                    // ReSharper disable once ExpressionIsAlwaysNull
                    Source = _completeMetadata
                };

                // ReSharper disable once PossibleNullReferenceException
                Log.Debug($"=== MetadataEditorViewModel: Source set ({_completeMetadata.Count} rows, {(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                ts = DateTime.Now;

                using (MetadataViewSource.DeferRefresh())
                {
                    FilterAgeRatings = filteredTypes.Contains(SettableFieldType.AgeRating) || !filteredTypes.Any();
                    FilterCategories = filteredTypes.Contains(SettableFieldType.Category) || !filteredTypes.Any();
                    FilterFeatures = filteredTypes.Contains(SettableFieldType.Feature) || !filteredTypes.Any();
                    FilterGenres = filteredTypes.Contains(SettableFieldType.Genre) || !filteredTypes.Any();
                    FilterSeries = filteredTypes.Contains(SettableFieldType.Series) || !filteredTypes.Any();
                    FilterTags = filteredTypes.Contains(SettableFieldType.Tag) || !filteredTypes.Any();

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

        public RelayCommand AddNewCommand => new RelayCommand(() =>
        {
            try
            {
                SettableFieldType type = SettableFieldType.Tag;
                string prefix = string.Empty;

                if (MetadataViewSource.View.CurrentItem != null)
                {
                    SettableMetadataObject templateItem = (SettableMetadataObject)MetadataViewSource.View.CurrentItem;

                    type = templateItem.Type;
                    prefix = templateItem.Prefix;
                }

                SettableMetadataObject newItem = CompleteMetadata.AddNewItem(type, prefix);

                if (newItem == null)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                newItem.Selected = true;

                MetadataViewSource.View.Filter = Filter;
                MetadataViewSource.View.MoveCurrentTo(newItem);
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

        public MetadataObjects CompleteMetadata
        {
            get => _completeMetadata;
            set => SetValue(ref _completeMetadata, value);
        }

        public bool FilterAgeRatings
        {
            get => _filterAgeRatings;
            set
            {
                SetValue(ref _filterAgeRatings, value);

                if (_filterAgeRatings)
                {
                    _filterTypes.Add(SettableFieldType.AgeRating);
                }
                else
                {
                    _filterTypes.Remove(SettableFieldType.AgeRating);
                }

                MetadataViewSource.View.Filter = Filter;
            }
        }

        public bool FilterCategories
        {
            get => _filterCategories;
            set
            {
                SetValue(ref _filterCategories, value);

                if (_filterCategories)
                {
                    _filterTypes.Add(SettableFieldType.Category);
                }
                else
                {
                    _filterTypes.Remove(SettableFieldType.Category);
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
                    _filterTypes.Add(SettableFieldType.Feature);
                }
                else
                {
                    _filterTypes.Remove(SettableFieldType.Feature);
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
                    _filterTypes.Add(SettableFieldType.Genre);
                }
                else
                {
                    _filterTypes.Remove(SettableFieldType.Genre);
                }

                MetadataViewSource.View.Filter = Filter;
            }
        }

        public string FilterPrefix
        {
            get => _filterPrefix;
            set
            {
                SetValue(ref _filterPrefix, value);

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
                    _filterTypes.Add(SettableFieldType.Series);
                }
                else
                {
                    _filterTypes.Remove(SettableFieldType.Series);
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
                    _filterTypes.Add(SettableFieldType.Tag);
                }
                else
                {
                    _filterTypes.Remove(SettableFieldType.Tag);
                }

                MetadataViewSource.View.Filter = Filter;
            }
        }

        public CollectionViewSource MetadataViewSource
        {
            get => _metadataViewSource;
            set => SetValue(ref _metadataViewSource, value);
        }

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set => SetValue(ref _plugin, value);
        }

        public ObservableCollection<string> Prefixes { get; } = new ObservableCollection<string>();

        public Visibility PrefixVisibility => _plugin.Settings.Settings.Prefixes?.Any() ?? false
            ? Visibility.Visible
            : Visibility.Collapsed;

        public string RuleName
        {
            get => _ruleName;
            set => SetValue(ref _ruleName, value);
        }

        public SettableFieldType RuleType
        {
            get => _ruleType;
            set => SetValue(ref _ruleType, value);
        }

        public RelayCommand<Window> SaveCommand => new RelayCommand<Window>(win =>
        {
            if (RuleName == null || RuleName?.Length == 0)
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

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                SetValue(ref _searchTerm, value);
                MetadataViewSource.View.Filter = Filter;
            }
        }

        public RelayCommand<object> SetAsTargetCommand => new RelayCommand<object>(item =>
        {
            RuleType = ((SettableMetadataObject)item).Type;
            RuleName = ((SettableMetadataObject)item).Name;
        });

        private bool Filter(object item)
            => item is SettableMetadataObject metadataObject && metadataObject.Name.RegExIsMatch(SearchTerm) &&
               _filterTypes.Contains(metadataObject.Type) && (!FilterSelected || metadataObject.Selected) &&
               (_filterPrefix == string.Empty || metadataObject.Prefix.Equals(_filterPrefix));
    }
}