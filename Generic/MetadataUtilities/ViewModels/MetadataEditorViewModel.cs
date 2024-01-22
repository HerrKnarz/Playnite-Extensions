using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MetadataUtilities
{
    public class MetadataEditorViewModel : ViewModelBase
    {
        private bool _filterCategories = true;

        private bool _filterFeatures = true;

        private bool _filterGenres = true;

        private bool _filterSeries = true;

        private bool _filterTags = true;

        private MetadataListObjects _metadataListObjects;

        private MetadataUtilities _plugin;

        private string _searchTerm = string.Empty;

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
                OnPropertyChanged("FilterCategories");
            }
        }

        public bool FilterFeatures
        {
            get => _filterFeatures;
            set
            {
                _filterFeatures = value;
                OnPropertyChanged("FilterFeatures");
            }
        }

        public bool FilterGenres
        {
            get => _filterGenres;
            set
            {
                _filterGenres = value;
                OnPropertyChanged("FilterGenres");
            }
        }

        public bool FilterSeries
        {
            get => _filterSeries;
            set
            {
                _filterSeries = value;
                OnPropertyChanged("FilterSeries");
            }
        }

        public bool FilterTags
        {
            get => _filterTags;
            set
            {
                _filterTags = value;
                OnPropertyChanged("FilterTags");
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged("SearchTerm");
            }
        }

        public RelayCommand FilterCommand => new RelayCommand(() =>
        {
            ExecuteFilter();
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
                        ExecuteFilter();
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
                MetadataListObjects.Remove(item);
            }
        }, items => items?.Any() ?? false);

        public MetadataListObjects MetadataListObjects
        {
            get => _metadataListObjects;
            set
            {
                _metadataListObjects = value;
                OnPropertyChanged("MetadataListObjects");
            }
        }

        public void ExecuteFilter()
        {
            List<MetadataListObject> filteredObjects = new List<MetadataListObject>();

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                ResourceProvider.GetString("LOCLoadingLabel"),
                false
            )
            {
                IsIndeterminate = true
            };

            API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
            {
                try
                {
                    if (FilterCategories)
                    {
                        filteredObjects.AddRange(API.Instance.Database.Categories
                            .Where(x => !SearchTerm.Any() || x.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).Select(category
                                => new MetadataListObject
                                {
                                    Id = category.Id,
                                    Name = category.Name,
                                    EditName = category.Name,
                                    GameCount = API.Instance.Database.Games.Count(g => g.CategoryIds?.Any(t => t == category.Id) ?? false),
                                    Type = FieldType.Category
                                }));
                    }

                    if (FilterFeatures)
                    {
                        filteredObjects.AddRange(API.Instance.Database.Features
                            .Where(x => !SearchTerm.Any() || x.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).Select(feature
                                => new MetadataListObject
                                {
                                    Id = feature.Id,
                                    Name = feature.Name,
                                    EditName = feature.Name,
                                    GameCount = API.Instance.Database.Games.Count(g => g.FeatureIds?.Any(t => t == feature.Id) ?? false),
                                    Type = FieldType.Feature
                                }));
                    }

                    if (FilterGenres)
                    {
                        filteredObjects.AddRange(API.Instance.Database.Genres
                            .Where(x => !SearchTerm.Any() || x.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).Select(genre
                                => new MetadataListObject
                                {
                                    Id = genre.Id,
                                    Name = genre.Name,
                                    EditName = genre.Name,
                                    GameCount = API.Instance.Database.Games.Count(g => g.GenreIds?.Any(t => t == genre.Id) ?? false),
                                    Type = FieldType.Genre
                                }));
                    }

                    if (FilterSeries)
                    {
                        filteredObjects.AddRange(API.Instance.Database.Series
                            .Where(x => !SearchTerm.Any() || x.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).Select(series
                                => new MetadataListObject
                                {
                                    Id = series.Id,
                                    Name = series.Name,
                                    EditName = series.Name,
                                    GameCount = API.Instance.Database.Games.Count(g => g.SeriesIds?.Any(t => t == series.Id) ?? false),
                                    Type = FieldType.Series
                                }));
                    }

                    if (FilterTags)
                    {
                        filteredObjects.AddRange(API.Instance.Database.Tags
                            .Where(x => !SearchTerm.Any() || x.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).Select(tag
                                => new MetadataListObject
                                {
                                    Id = tag.Id,
                                    Name = tag.Name,
                                    EditName = tag.Name,
                                    GameCount = API.Instance.Database.Games.Count(g => g.TagIds?.Any(t => t == tag.Id) ?? false),
                                    Type = FieldType.Tag
                                }));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            MetadataListObjects.Clear();
            MetadataListObjects.AddMissing(filteredObjects.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name));
        }


        private void InitializeView(MetadataUtilities plugin)
        {
            _plugin = plugin;
            MetadataListObjects = new MetadataListObjects();
        }
    }
}