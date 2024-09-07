using KNARZhelper;
using KNARZhelper.Enum;
using MetadataUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;

namespace MetadataUtilities.ViewModels
{
    public class MergeRuleEditorViewModel : ObservableObject
    {
        private MetadataObjects _completeMetadata;
        private string _filterPrefix = string.Empty;
        private bool _filterSelected;
        private ObservableCollection<FilterType> _filterTypes;
        private CollectionViewSource _metadataViewSource;
        private MetadataUtilities _plugin;
        private string _ruleName = string.Empty;
        private FieldType _ruleType = FieldType.Category;
        private string _searchTerm = string.Empty;

        public MergeRuleEditorViewModel(MetadataUtilities plugin, MetadataObjects objects)
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
                    _filterTypes = plugin.Settings.Settings.FilterTypes
                        .Select(x => new FilterType() { Type = x.Type }).ToObservable();

                    foreach (FilterType filterType in FilterTypes)
                    {
                        filterType.PropertyChanged += (x, y) => MetadataViewSource.View.Filter = Filter;

                        filterType.Selected = CompleteMetadata.Any(x => x.Selected && x.Type == filterType.Type);
                    }

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
                FieldType type = FieldType.Tag;
                string prefix = string.Empty;

                if (MetadataViewSource.View.CurrentItem != null)
                {
                    MetadataObject templateItem = (MetadataObject)MetadataViewSource.View.CurrentItem;

                    type = templateItem.Type;
                    prefix = templateItem.Prefix;
                }

                MetadataObject newItem = CompleteMetadata.AddNewItem(type, prefix);

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

        public Dictionary<FieldType, string> FieldValuePairs => FieldTypeHelper.ItemListFieldValues();

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

        public ObservableCollection<FilterType> FilterTypes
        {
            get => _filterTypes;
            set => SetValue(ref _filterTypes, value);
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

        public Visibility PrefixVisibility => (_plugin.Settings.Settings.Prefixes?.Count ?? 0) > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

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
            RuleType = ((MetadataObject)item).Type;
            RuleName = ((MetadataObject)item).Name;
        });

        private bool Filter(object item)
            => item is MetadataObject metadataObject && metadataObject.Name.RegExIsMatch(SearchTerm) &&
               FilterTypes.Any(x => x.Selected && x.Type == metadataObject.Type) &&
               (!FilterSelected || metadataObject.Selected) &&
               (_filterPrefix == string.Empty || metadataObject.Prefix.Equals(_filterPrefix));
    }
}