using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MetadataUtilities
{
    public class MetadataEditorViewModel : ViewModelBase
    {
        //TODO: Add MergeRule property and populate grid and rule name/type fields with it.

        private MetadataListObjects _completeMetadata;
        private bool _filterCategories = true;
        private bool _filterFeatures = true;
        private bool _filterGenres = true;
        private bool _filterSelected;
        private bool _filterSeries = true;
        private bool _filterTags = true;
        private bool _isSelectMode;
        private MergeRule _mergeRule;
        private MetadataUtilities _plugin;
        private string _ruleName = string.Empty;
        private FieldType _ruleType = FieldType.Category;
        private string _searchTerm = string.Empty;

        public MetadataEditorViewModel()
        {
            _completeMetadata = new MetadataListObjects();
            _completeMetadata.LoadMetadata();

            Metadata = CollectionViewSource.GetDefaultView(_completeMetadata);

            Metadata.Filter = Filter;
        }

        public MergeRule MergeRule
        {
            get => _mergeRule;
            set
            {
                _mergeRule = value;

                if (value == null)
                {
                    return;
                }

                RuleName = value.Name;
                RuleType = value.Type;

                foreach (MetadataListObject item in value.SourceObjects)
                {
                    MetadataListObject foundItem = CompleteMetadata.FirstOrDefault(x => x.Name == item.Name && x.Type == item.Type);

                    if (foundItem != null)
                    {
                        foundItem.Selected = true;
                    }
                    else
                    {
                        item.Selected = true;
                        CompleteMetadata.Add(item);
                    }
                }

                Metadata.Refresh();
            }
        }

        public Visibility VisibleInEditorMode => IsSelectMode ? Visibility.Collapsed : Visibility.Visible;

        public Visibility VisibleInSelectMode => IsSelectMode ? Visibility.Visible : Visibility.Collapsed;

        public bool IsSelectMode
        {
            get => _isSelectMode;
            set
            {
                _isSelectMode = value;
                OnPropertyChanged("IsSelectMode");
                OnPropertyChanged("SelectionMode");
                OnPropertyChanged("VisibleInEditorMode");
                OnPropertyChanged("VisibleInSelectMode");
            }
        }

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set => InitializeView(value);
        }

        public bool FilterCategories
        {
            get => _filterCategories;
            set
            {
                _filterCategories = value;
                Metadata.Refresh();
                OnPropertyChanged("FilterCategories");
            }
        }

        public bool FilterFeatures
        {
            get => _filterFeatures;
            set
            {
                _filterFeatures = value;
                Metadata.Refresh();
                OnPropertyChanged("FilterFeatures");
            }
        }

        public bool FilterGenres
        {
            get => _filterGenres;
            set
            {
                _filterGenres = value;
                Metadata.Refresh();
                OnPropertyChanged("FilterGenres");
            }
        }

        public bool FilterSelected
        {
            get => _filterSelected;
            set
            {
                _filterSelected = value;
                Metadata.Refresh();
                OnPropertyChanged("FilterSelected");
            }
        }

        public bool FilterSeries
        {
            get => _filterSeries;
            set
            {
                _filterSeries = value;
                Metadata.Refresh();
                OnPropertyChanged("FilterSeries");
            }
        }

        public bool FilterTags
        {
            get => _filterTags;
            set
            {
                _filterTags = value;
                Metadata.Refresh();
                OnPropertyChanged("FilterTags");
            }
        }

        public string RuleName
        {
            get => _ruleName;
            set
            {
                _ruleName = value;
                OnPropertyChanged("RuleName");
            }
        }

        public FieldType RuleType
        {
            get => _ruleType;
            set
            {
                _ruleType = value;
                OnPropertyChanged("RuleType");
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                Metadata.Refresh();
                OnPropertyChanged("SearchTerm");
            }
        }

        public SelectionMode SelectionMode => IsSelectMode ? SelectionMode.Single : SelectionMode.Extended;

        public RelayCommand AddNewCommand => new RelayCommand(() =>
        {
            try
            {
                MetadataListObject newItem = new MetadataListObject();

                AddNewObjectView newObjectViewView = new AddNewObjectView(Plugin, newItem);

                Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAddNewObject"));

                window.Content = newObjectViewView;

                if (window.ShowDialog() ?? false)
                {
                    if (CompleteMetadata.Any(x => x.Type == newItem.Type && x.EditName == newItem.Name))
                    {
                        return;
                    }

                    if (IsSelectMode)
                    {
                        newItem.Selected = true;
                    }
                    else
                    {
                        newItem.Id = DatabaseObjectHelper.AddDbObject(newItem.Type, newItem.Name);
                    }

                    newItem.EditName = newItem.Name;
                    CompleteMetadata.Add(newItem);

                    Metadata.Refresh();
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing merge dialog", true);
            }
        });

        public RelayCommand<IList<object>> MergeItemsCommand => new RelayCommand<IList<object>>(items =>
        {
            if ((items?.Count() ?? 0) > 1)
            {
                try
                {
                    MetadataListObjects mergeItems = new MetadataListObjects();

                    mergeItems.AddMissing(items.ToList().Cast<MetadataListObject>());

                    MergeDialogView mergeView = new MergeDialogView(Plugin, mergeItems);

                    Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesEditorMerge"));

                    window.Content = mergeView;

                    if (window.ShowDialog() ?? false)
                    {
                        foreach (MetadataListObject itemToRemove in mergeItems)
                        {
                            if (!DatabaseObjectHelper.DBObjectExists(itemToRemove.EditName, itemToRemove.Type))
                            {
                                CompleteMetadata.Remove(itemToRemove);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error during initializing merge dialog", true);
                }

                return;
            }

            API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogMultipleSelected"), string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
        }, items => items?.Any() ?? false);

        public RelayCommand<IList<object>> RemoveItemsCommand => new RelayCommand<IList<object>>(items =>
        {
            using (API.Instance.Database.BufferedUpdate())
            {
                GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                    ResourceProvider.GetString("LOCMetadataUtilitiesDialogRemovingItems"),
                    false
                )
                {
                    IsIndeterminate = false
                };

                API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                {
                    try
                    {
                        activateGlobalProgress.ProgressMaxValue = items.Count;

                        foreach (MetadataListObject item in items)
                        {
                            DatabaseObjectHelper.RemoveDbObject(item.Type, item.Id);

                            activateGlobalProgress.CurrentProgressValue++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }, globalProgressOptions);
            }

            foreach (MetadataListObject item in items.ToList().Cast<MetadataListObject>())
            {
                CompleteMetadata.Remove(item);
            }
        }, items => items?.Any() ?? false);

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

            MergeRule.Name = RuleName;
            MergeRule.Type = RuleType;
            MergeRule.SourceObjects.Clear();
            MergeRule.SourceObjects.AddMissing(CompleteMetadata.Where(x => x.Selected).ToList());

            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public RelayCommand<Window> CancelCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = false;
            win.Close();
        }, win => win != null);

        public RelayCommand<object> SetAsTargetCommand => new RelayCommand<object>(item =>
        {
            RuleType = ((MetadataListObject)item).Type;
            RuleName = ((MetadataListObject)item).Name;
        });

        public ICollectionView Metadata { get; }

        public MetadataListObjects CompleteMetadata
        {
            get => _completeMetadata;
            set
            {
                _completeMetadata = value;
                OnPropertyChanged("CompleteMetadata");
            }
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

        private void InitializeView(MetadataUtilities plugin, bool isSelectMode = false)
        {
            _plugin = plugin;
        }
    }
}