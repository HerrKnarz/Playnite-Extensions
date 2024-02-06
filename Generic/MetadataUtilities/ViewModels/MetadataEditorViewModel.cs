using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MetadataUtilities
{
    public class MetadataEditorViewModel : ObservableObject
    {
        private readonly HashSet<FieldType> _filterTypes = new HashSet<FieldType>();
        private readonly bool _showRelatedGames = true;
        private int _categoryCount;
        private MetadataListObjects _completeMetadata;
        private int _featureCount;
        private bool _filterCategories = true;
        private bool _filterFeatures = true;
        private bool _filterGenres = true;
        private bool _filterSeries = true;
        private bool _filterTags = true;
        private ObservableCollection<MyGame> _games = new ObservableCollection<MyGame>();
        private CollectionViewSource _gamesViewSource;
        private int _genreCount;
        private CollectionViewSource _metadataViewSource;
        private MetadataUtilities _plugin;
        private string _searchTerm = string.Empty;
        private int _seriesCount;
        private int _tagCount;

        public MetadataEditorViewModel(MetadataUtilities plugin, MetadataListObjects objects)
        {
            Log.Debug("=== MetadataEditorViewModel: Start ===");
            DateTime ts = DateTime.Now;


            Plugin = plugin;
            CompleteMetadata = objects;

            CalculateItemCount();

            GamesViewSource = new CollectionViewSource
            {
                Source = _games
            };

            GamesViewSource.SortDescriptions.Add(new SortDescription("RealSortingName", ListSortDirection.Ascending));
            GamesViewSource.IsLiveSortingRequested = true;

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
                MetadataViewSource.View.Filter = Filter;

                MetadataViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
                MetadataViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                MetadataViewSource.IsLiveSortingRequested = true;

                Log.Debug($"=== MetadataEditorViewModel: Sort set ({_completeMetadata.Count} rows, {(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                ts = DateTime.Now;

                FilterCategories = Plugin.Settings.Settings.FilterCategories;
                FilterFeatures = Plugin.Settings.Settings.FilterFeatures;
                FilterGenres = Plugin.Settings.Settings.FilterGenres;
                FilterSeries = Plugin.Settings.Settings.FilterSeries;
                FilterTags = Plugin.Settings.Settings.FilterTags;

                MetadataViewSource.View.CurrentChanged += CurrentChanged;
            }

            Log.Debug($"=== MetadataEditorViewModel: Filter set ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
            ts = DateTime.Now;

            MetadataViewSource.View.MoveCurrentToFirst();

            Log.Debug($"=== MetadataEditorViewModel: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
        }

        public int CategoryCount
        {
            get => _categoryCount;
            set => SetValue(ref _categoryCount, value);
        }

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set => SetValue(ref _plugin, value);
        }

        public int FeatureCount
        {
            get => _featureCount;
            set => SetValue(ref _featureCount, value);
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

                MetadataViewSource.View.Refresh();
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

                MetadataViewSource.View.Refresh();
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

                MetadataViewSource.View.Refresh();
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

                MetadataViewSource.View.Refresh();
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

                MetadataViewSource.View.Refresh();
            }
        }

        public ObservableCollection<MyGame> Games
        {
            get => _games;
            set => SetValue(ref _games, value);
        }

        public CollectionViewSource GamesViewSource
        {
            get => _gamesViewSource;
            set => SetValue(ref _gamesViewSource, value);
        }

        public int GenreCount
        {
            get => _genreCount;
            set => SetValue(ref _genreCount, value);
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

        public int SeriesCount
        {
            get => _seriesCount;
            set => SetValue(ref _seriesCount, value);
        }

        public int TagCount
        {
            get => _tagCount;
            set => SetValue(ref _tagCount, value);
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

                    newItem.Id = DatabaseObjectHelper.AddDbObject(newItem.Type, newItem.Name);
                    newItem.EditName = newItem.Name;
                    CompleteMetadata.Add(newItem);

                    CalculateItemCount();

                    MetadataViewSource.View.Refresh();
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing merge dialog", true);
            }
        });

        public RelayCommand<IList<object>> ChangeTypeCommand => new RelayCommand<IList<object>>(items =>
        {
            try
            {
                Plugin.IsUpdating = true;

                MetadataListObjects changeItems = new MetadataListObjects(Plugin.Settings.Settings);

                changeItems.AddMissing(items.ToList().Cast<MetadataListObject>());


                ChangeTypeViewModel viewModel = new ChangeTypeViewModel(Plugin, changeItems);

                ChangeTypeView view = new ChangeTypeView();

                Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesDialogChangeType"));
                window.Content = view;
                window.DataContext = viewModel;

                if (window.ShowDialog() ?? false)
                {
                    foreach (MetadataListObject itemToRemove in changeItems)
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

                    foreach (MetadataListObject itemToAdd in viewModel.NewObjects)
                    {
                        itemToAdd.GetGameCount(Plugin.Settings.Settings.IgnoreHiddenGamesInGameCount);
                    }

                    CompleteMetadata.AddMissing(viewModel.NewObjects);

                    CalculateItemCount();
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing change type dialog", true);
            }
            finally
            {
                Plugin.IsUpdating = false;
            }
        }, items => items?.Any() ?? false);

        public RelayCommand<IList<object>> MergeItemsCommand => new RelayCommand<IList<object>>(items =>
        {
            if ((int)items?.Count() > 1)
            {
                try
                {
                    Plugin.IsUpdating = true;

                    MetadataListObjects mergeItems = new MetadataListObjects(Plugin.Settings.Settings);

                    mergeItems.AddMissing(items.ToList().Cast<MetadataListObject>());

                    MergeDialogViewModel viewModel = new MergeDialogViewModel(Plugin, mergeItems);

                    MergeDialogView mergeView = new MergeDialogView();

                    Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesEditorMerge"));
                    window.Content = mergeView;
                    window.DataContext = viewModel;

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

                        CalculateItemCount();
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

                CalculateItemCount();
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

            CalculateItemCount();

            foreach (MetadataListObject itemToRemove in itemsToRemove)
            {
                CompleteMetadata.Remove(itemToRemove);
            }
        });

        public RelayCommand<Window> CloseCommand => new RelayCommand<Window>(win =>
        {
            Plugin.Settings.Settings.FilterCategories = FilterCategories;
            Plugin.Settings.Settings.FilterFeatures = FilterFeatures;
            Plugin.Settings.Settings.FilterGenres = FilterGenres;
            Plugin.Settings.Settings.FilterSeries = FilterSeries;
            Plugin.Settings.Settings.FilterTags = FilterTags;
            Plugin.Settings.Settings.EditorWindowHeight = Convert.ToInt32(win.Height);
            Plugin.Settings.Settings.EditorWindowWidth = Convert.ToInt32(win.Width);
            Plugin.SavePluginSettings(Plugin.Settings.Settings);

            win.DialogResult = true;
            win.Close();
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

        private void CurrentChanged(object sender, EventArgs e)
        {
            if (!_showRelatedGames)
            {
                return;
            }

            Log.Debug("=== CurrentChanged: Start ===");
            DateTime ts = DateTime.Now;

            Games.Clear();

            MetadataListObject currItem = (MetadataListObject)_metadataViewSource.View.CurrentItem;

            if (currItem == null)
            {
                return;
            }

            foreach (Game game in API.Instance.Database.Games.Where(g =>
                !(Plugin.Settings.Settings.IgnoreHiddenGamesInGameCount && g.Hidden) && (
                    (currItem.Type == FieldType.Category && (g.CategoryIds?.Contains(currItem.Id) ?? false)) ||
                    (currItem.Type == FieldType.Feature && (g.FeatureIds?.Contains(currItem.Id) ?? false)) ||
                    (currItem.Type == FieldType.Genre && (g.GenreIds?.Contains(currItem.Id) ?? false)) ||
                    (currItem.Type == FieldType.Series && (g.SeriesIds?.Contains(currItem.Id) ?? false)) ||
                    (currItem.Type == FieldType.Tag && (g.TagIds?.Contains(currItem.Id) ?? false)))
            ).OrderBy(g => g.Name).ToList())
            {
                Games.Add(new MyGame
                {
                    Name = game.Name,
                    SortingName = game.SortingName,
                    CompletionStatus = game.CompletionStatus,
                    ReleaseYear = game.ReleaseYear
                });
            }

            Log.Debug($"=== CurrentChanged: Start refresh ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
            ts = DateTime.Now;

            GamesViewSource.View.Refresh();

            Log.Debug($"=== CurrentChanged: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
        }

        public void CalculateItemCount()
        {
            CategoryCount = (int)API.Instance.Database.Categories?.Count;
            FeatureCount = (int)API.Instance.Database.Features?.Count;
            GenreCount = (int)API.Instance.Database.Genres?.Count;
            SeriesCount = (int)API.Instance.Database.Series?.Count;
            TagCount = (int)API.Instance.Database.Tags?.Count;
        }

        private bool Filter(object item)
        {
            MetadataListObject metadataListObject = item as MetadataListObject;

            return metadataListObject.EditName.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase) &&
                   _filterTypes.Contains(metadataListObject.Type);
        }
    }
}