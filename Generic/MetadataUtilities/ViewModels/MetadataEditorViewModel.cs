using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MetadataUtilities
{
    public class MetadataEditorViewModel : ViewModelBase
    {
        private MetadataListObjects _completeMetadata;

        private bool _filterCategories = true;

        private bool _filterFeatures = true;

        private bool _filterGenres = true;

        private bool _filterSeries = true;

        private bool _filterTags = true;

        private MetadataUtilities _plugin;

        private string _searchTerm = string.Empty;

        public MetadataEditorViewModel()
        {
            _completeMetadata = new MetadataListObjects();
            _completeMetadata.LoadMetadata();

            Metadata = CollectionViewSource.GetDefaultView(_completeMetadata);

            Metadata.Filter = Filter;
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
                    Guid id = DatabaseObjectHelper.AddDbObject(newItem.Type, newItem.Name);

                    if (CompleteMetadata.Any(x => x.Id == id))
                    {
                        return;
                    }

                    newItem.Id = id;
                    newItem.EditName = newItem.Name;
                    CompleteMetadata.Add(newItem);
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

            API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogMultipleSelected"));
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

            return metadataListObject.EditName.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase) && types.Any(t => t == metadataListObject.Type);
        }

        private void InitializeView(MetadataUtilities plugin)
        {
            _plugin = plugin;
        }
    }
}