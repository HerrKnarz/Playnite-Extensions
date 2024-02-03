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
    public class MetadataEditorViewModel : ObservableObject
    {
        private MetadataListObjects _completeMetadata;
        private bool _filterCategories = true;
        private bool _filterFeatures = true;
        private bool _filterGenres = true;
        private bool _filterSelected;
        private bool _filterSeries = true;
        private bool _filterTags = true;
        private bool _isSelectMode;
        private CollectionViewSource _metadataViewSource;
        private MetadataUtilities _plugin;
        private string _ruleName = string.Empty;
        private FieldType _ruleType = FieldType.Category;
        private string _searchTerm = string.Empty;

        public MetadataEditorViewModel(MetadataUtilities plugin, MetadataListObjects objects, bool isSelectMode = false)
        {
            Plugin = plugin;
            IsSelectMode = isSelectMode;
            CompleteMetadata = objects;

            MetadataViewSource = new CollectionViewSource
            {
                Source = _completeMetadata
            };

            MetadataViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            MetadataViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            MetadataViewSource.IsLiveSortingRequested = true;
            MetadataViewSource.View.Filter = Filter;
        }

        public Visibility VisibleInEditorMode => IsSelectMode ? Visibility.Collapsed : Visibility.Visible;

        public Visibility VisibleInSelectMode => IsSelectMode ? Visibility.Visible : Visibility.Collapsed;

        public bool IsSelectMode
        {
            get => _isSelectMode;
            set
            {
                _isSelectMode = value;
                SetValue(ref _isSelectMode, value);
                OnPropertyChanged("SelectionMode");
                OnPropertyChanged("VisibleInEditorMode");
                OnPropertyChanged("VisibleInSelectMode");
            }
        }

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set
            {
                _plugin = value;
                SetValue(ref _plugin, value);
            }
        }

        public bool FilterCategories
        {
            get => _filterCategories;
            set
            {
                _filterCategories = value;
                MetadataViewSource.View.Refresh();
                SetValue(ref _filterCategories, value);
            }
        }

        public bool FilterFeatures
        {
            get => _filterFeatures;
            set
            {
                _filterFeatures = value;
                MetadataViewSource.View.Refresh();
                SetValue(ref _filterFeatures, value);
            }
        }

        public bool FilterGenres
        {
            get => _filterGenres;
            set
            {
                _filterGenres = value;
                MetadataViewSource.View.Refresh();
                SetValue(ref _filterGenres, value);
            }
        }

        public bool FilterSelected
        {
            get => _filterSelected;
            set
            {
                _filterSelected = value;
                MetadataViewSource.View.Refresh();
                SetValue(ref _filterSelected, value);
            }
        }

        public bool FilterSeries
        {
            get => _filterSeries;
            set
            {
                _filterSeries = value;
                MetadataViewSource.View.Refresh();
                SetValue(ref _filterSeries, value);
            }
        }

        public bool FilterTags
        {
            get => _filterTags;
            set
            {
                _filterTags = value;
                MetadataViewSource.View.Refresh();
                SetValue(ref _filterTags, value);
            }
        }

        public string RuleName
        {
            get => _ruleName;
            set
            {
                _ruleName = value;
                SetValue(ref _ruleName, value);
            }
        }

        public FieldType RuleType
        {
            get => _ruleType;
            set
            {
                _ruleType = value;
                SetValue(ref _ruleType, value);
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                MetadataViewSource.View.Refresh();
                SetValue(ref _searchTerm, value);
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

                    MetadataViewSource.View.Refresh();
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
                    Plugin.IsUpdating = true;

                    MetadataListObjects mergeItems = new MetadataListObjects(Plugin.Settings.Settings);

                    mergeItems.AddMissing(items.ToList().Cast<MetadataListObject>());

                    MergeDialogView mergeView = new MergeDialogView(Plugin, mergeItems);

                    Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesEditorMerge"));

                    window.Content = mergeView;

                    if (window.ShowDialog() ?? false)
                    {
                        foreach (MetadataListObject itemToRemove in mergeItems)
                        {
                            if (!DatabaseObjectHelper.DbObjectExists(itemToRemove.EditName, itemToRemove.Type))
                            {
                                CompleteMetadata.Remove(itemToRemove);
                            }
                            else
                            {
                                itemToRemove.GetGameCount(Plugin.Settings.Settings.IgnoreHiddenGamesInGameCount);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error during initializing merge dialog", true);
                }
                finally
                {
                    Plugin.IsUpdating = false;
                }

                return;
            }

            API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogMultipleSelected"), ResourceProvider.GetString("LOCMetadataUtilitiesName"), MessageBoxButton.OK, MessageBoxImage.Information);
        }, items => items?.Any() ?? false);

        public RelayCommand<IList<object>> RemoveItemsCommand => new RelayCommand<IList<object>>(items =>
        {
            Plugin.IsUpdating = true;
            try
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
            }
            finally
            {
                Plugin.IsUpdating = false;
            }
        }, items => items?.Any() ?? false);

        public RelayCommand RemoveUnusedCommand => new RelayCommand(() =>
        {
            List<MetadataListObject> removedItems = MetadataListObjects.RemoveUnusedMetadata(false, Plugin.Settings.Settings.IgnoreHiddenGamesInRemoveUnused);

            if (!removedItems.Any())
            {
                return;
            }

            List<MetadataListObject> itemsToRemove = CompleteMetadata.Where(x => removedItems.Any(y => x.Type == y.Type && x.EditName == y.Name)).ToList();

            foreach (MetadataListObject itemToRemove in itemsToRemove)
            {
                CompleteMetadata.Remove(itemToRemove);
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
            set
            {
                _metadataViewSource = value;
                OnPropertyChanged();
            }
        }

        public MetadataListObjects CompleteMetadata
        {
            get => _completeMetadata;
            set
            {
                _completeMetadata = value;
                OnPropertyChanged();
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
    }
}