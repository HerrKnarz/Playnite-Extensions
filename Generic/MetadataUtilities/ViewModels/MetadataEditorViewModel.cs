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
using System.Windows.Forms;

namespace MetadataUtilities
{
    public class MetadataEditorViewModel : ObservableObject, IEditableObject
    {
        private readonly HashSet<FieldType> _filterTypes = new HashSet<FieldType>();
        private readonly bool _showRelatedGames;
        private int _categoryCount;
        private MetadataObjects _completeMetadata;
        private int _featureCount;
        private bool _filterCategories = true;
        private bool _filterFeatures = true;
        private bool _filterGenres = true;
        private string _filterPrefix = string.Empty;
        private bool _filterSeries = true;
        private bool _filterTags = true;
        private ObservableCollection<MyGame> _games = new ObservableCollection<MyGame>();
        private CollectionViewSource _gamesViewSource;
        private int _genreCount;
        private bool _groupMatches;
        private CollectionViewSource _metadataViewSource;
        private MetadataUtilities _plugin;
        private string _searchTerm = string.Empty;
        private int _seriesCount;
        private int _tagCount;
        private List<MetadataObject> _selectedItems = new List<MetadataObject>();

        public MetadataEditorViewModel(MetadataUtilities plugin, MetadataObjects objects)
        {
            Log.Debug("=== MetadataEditorViewModel: Start ===");
            DateTime ts = DateTime.Now;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Plugin = plugin;
                CompleteMetadata = objects;

                Prefixes.Add(string.Empty);
                Prefixes.AddMissing(Plugin.Settings.Settings.Prefixes);

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
                    MetadataViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
                    MetadataViewSource.SortDescriptions.Add(new SortDescription("Prefix", ListSortDirection.Ascending));
                    MetadataViewSource.SortDescriptions.Add(new SortDescription("EditName", ListSortDirection.Ascending));
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

                _showRelatedGames = true;

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

                ((IEditableCollectionView)MetadataViewSource.View).CommitEdit();
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

                ((IEditableCollectionView)MetadataViewSource.View).CommitEdit();
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

                ((IEditableCollectionView)MetadataViewSource.View).CommitEdit();
                MetadataViewSource.View.Filter = Filter;
            }
        }

        public string FilterPrefix
        {
            get => _filterPrefix;
            set
            {
                SetValue(ref _filterPrefix, value);

                ((IEditableCollectionView)MetadataViewSource.View).CommitEdit();
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

                ((IEditableCollectionView)MetadataViewSource.View).CommitEdit();
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

                ((IEditableCollectionView)MetadataViewSource.View).CommitEdit();
                MetadataViewSource.View.Filter = Filter;
            }
        }

        public Visibility GameGridCompletionStatusVisibility => _plugin.Settings.Settings.GameGridShowCompletionStatus
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility GameGridPlatformVisibility => _plugin.Settings.Settings.GameGridShowPlatform
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility GameGridReleaseVisibility => _plugin.Settings.Settings.GameGridShowReleaseYear
            ? Visibility.Visible
            : Visibility.Collapsed;

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

        public bool GroupMatches
        {
            get => _groupMatches;
            set
            {
                SetValue(ref _groupMatches, value);

                if (_groupMatches)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        MetadataObjects.UpdateGroupDisplay(CompleteMetadata.ToList());
                        MetadataViewSource.View.GroupDescriptions.Add(new PropertyGroupDescription("CleanedUpName"));
                        ((IEditableCollectionView)MetadataViewSource.View).CommitEdit();
                        MetadataViewSource.View.Filter = Filter;
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }
                else
                {
                    MetadataViewSource.View.GroupDescriptions.Clear();
                    ((IEditableCollectionView)MetadataViewSource.View).CommitEdit();
                    MetadataViewSource.View.Filter = Filter;
                }
            }
        }

        public ObservableCollection<string> Prefixes { get; } = new ObservableCollection<string>();

        public Visibility PrefixVisibility => _plugin.Settings.Settings.Prefixes?.Any() ?? false
            ? Visibility.Visible
            : Visibility.Collapsed;

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                SetValue(ref _searchTerm, value);
                ((IEditableCollectionView)MetadataViewSource.View).CommitEdit();
                MetadataViewSource.View.Filter = Filter;
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
                MetadataObject newItem = new MetadataObject(_plugin.Settings.Settings);

                if (MetadataViewSource.View.CurrentItem != null)
                {
                    MetadataObject templateItem = (MetadataObject)MetadataViewSource.View.CurrentItem;

                    newItem.Type = templateItem.Type;
                    newItem.Prefix = templateItem.Prefix;
                }

                Window window = AddNewObjectViewModel.GetWindow(Plugin, newItem);

                if (window == null)
                {
                    return;
                }

                if (!(window.ShowDialog() ?? false))
                {
                    return;
                }

                if (CompleteMetadata.Any(x => x.Type == newItem.Type && x.Name == newItem.Name))
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                newItem.Id = DatabaseObjectHelper.AddDbObject(newItem.Type, newItem.Name);
                newItem.Name = newItem.Name;

                CompleteMetadata.Add(newItem);

                MetadataObjects.UpdateGroupDisplay(CompleteMetadata.ToList());

                CalculateItemCount();

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

        public RelayCommand<IList<object>> ChangeTypeCommand => new RelayCommand<IList<object>>(items =>
        {
            if (items == null || items.Count < 1)
            {
                return;
            }

            try
            {
                Plugin.IsUpdating = true;

                MetadataObjects changeItems = new MetadataObjects(Plugin.Settings.Settings);

                changeItems.AddMissing(items.ToList().Cast<MetadataObject>());

                ChangeTypeViewModel viewModel = new ChangeTypeViewModel(Plugin, changeItems);

                ChangeTypeView view = new ChangeTypeView();

                Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesDialogChangeType"));
                window.Content = view;
                window.DataContext = viewModel;

                if (!(window.ShowDialog() ?? false))
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                foreach (MetadataObject itemToRemove in changeItems)
                {
                    if (!DatabaseObjectHelper.DbObjectExists(itemToRemove.Name, itemToRemove.Type))
                    {
                        CompleteMetadata.Remove(itemToRemove);
                    }
                    else
                    {
                        itemToRemove.GetGameCount();
                    }
                }

                foreach (MetadataObject itemToAdd in viewModel.NewObjects)
                {
                    itemToAdd.GetGameCount();
                }

                CompleteMetadata.AddMissing(viewModel.NewObjects);

                MetadataObjects.UpdateGroupDisplay(CompleteMetadata.ToList());

                CalculateItemCount();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing change type dialog", true);
            }
            finally
            {
                Plugin.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }, items => items?.Count > 0);

        public RelayCommand<IList<object>> MergeItemsCommand => new RelayCommand<IList<object>>(items =>
        {
            if (items == null || items.Count < 2)
            {
                API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogMultipleSelected"), ResourceProvider.GetString("LOCMetadataUtilitiesName"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Plugin.IsUpdating = true;

                MetadataObjects mergeItems = new MetadataObjects(Plugin.Settings.Settings);

                mergeItems.AddMissing(items.ToList().Cast<MetadataObject>());

                MergeDialogViewModel viewModel = new MergeDialogViewModel(Plugin, mergeItems);

                MergeDialogView mergeView = new MergeDialogView();

                Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesEditorMerge"));
                window.Content = mergeView;
                window.DataContext = viewModel;

                if (!(window.ShowDialog() ?? false))
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                foreach (MetadataObject itemToRemove in mergeItems)
                {
                    if (!DatabaseObjectHelper.DbObjectExists(itemToRemove.Name, itemToRemove.Type))
                    {
                        CompleteMetadata.Remove(itemToRemove);
                    }
                    else
                    {
                        itemToRemove.GetGameCount();
                    }
                }

                MetadataObjects.UpdateGroupDisplay(CompleteMetadata.ToList());

                CalculateItemCount();

                LoadRelatedGames();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing merge dialog", true);
            }
            finally
            {
                Plugin.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }, items => items?.Count > 1);

        public RelayCommand<IList<object>> RemoveItemsCommand => new RelayCommand<IList<object>>(items =>
        {
            if (items == null || items.Count < 1)
            {
                return;
            }

            Plugin.IsUpdating = true;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                using (API.Instance.Database.BufferedUpdate())
                {
                    GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                        ResourceProvider.GetString("LOCMetadataUtilitiesProgressRemovingItems"),
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

                            foreach (MetadataObject item in items.Cast<MetadataObject>())
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

                foreach (MetadataObject item in items.ToList().Cast<MetadataObject>())
                {
                    CompleteMetadata.Remove(item);
                }

                MetadataObjects.UpdateGroupDisplay(CompleteMetadata.ToList());

                CalculateItemCount();
            }
            finally
            {
                Plugin.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }, items => items != null && items?.Count > 0);

        public RelayCommand<IList<object>> RemoveGamesCommand => new RelayCommand<IList<object>>(items =>
        {
            Plugin.IsUpdating = true;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                MetadataObject selectedItem = (MetadataObject)MetadataViewSource.View.CurrentItem;

                if (selectedItem == null)
                {
                    return;
                }

                List<Game> games = items.Cast<MyGame>().Select(x => x.Game).ToList();

                if (games.Count == 0)
                {
                    return;
                }

                List<Guid> gamesAffected = DatabaseObjectHelper.ReplaceDbObject(games, selectedItem.Type, selectedItem.Id, null, null, false).ToList();

                if (gamesAffected.Count == 0)
                {
                    return;
                }

                LoadRelatedGames();
                selectedItem.GetGameCount();

                Cursor.Current = Cursors.Default;
                API.Instance.Dialogs.ShowMessage(string.Format(
                    ResourceProvider.GetString("LOCMetadataUtilitiesDialogRemovedMetadataMessage"),
                    selectedItem.TypeAndName, gamesAffected.Distinct().Count()));
            }
            finally
            {
                Plugin.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }, items => items != null && items?.Count > 0);

        public RelayCommand RemoveUnusedCommand => new RelayCommand(() =>
        {
            Cursor.Current = Cursors.WaitCursor;
            Plugin.IsUpdating = true;

            try
            {
                List<MetadataObject> removedItems = MetadataObjects.RemoveUnusedMetadata(_plugin.Settings.Settings);

                if (removedItems.Count == 0)
                {
                    return;
                }

                List<MetadataObject> itemsToRemove = CompleteMetadata
                    .Where(x => removedItems.Any(y => x.Type == y.Type && x.Name == y.Name)).ToList();

                CalculateItemCount();

                foreach (MetadataObject itemToRemove in itemsToRemove)
                {
                    CompleteMetadata.Remove(itemToRemove);
                }

                MetadataObjects.UpdateGroupDisplay(CompleteMetadata.ToList());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                Plugin.IsUpdating = false;
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

        public MetadataObjects CompleteMetadata
        {
            get => _completeMetadata;
            set => SetValue(ref _completeMetadata, value);
        }

        public List<MetadataObject> SelectedItems
        {
            get => _selectedItems;
            set => SetValue(ref _selectedItems, value);
        }

        public void BeginEdit() { }

        public void EndEdit() { }

        public void CancelEdit() { }

        private void CurrentChanged(object sender, EventArgs e) => LoadRelatedGames();

        private void LoadRelatedGames()
        {
            if (!_showRelatedGames)
            {
                return;
            }

            Games.Clear();

            MetadataObject currentItem = (MetadataObject)_metadataViewSource.View.CurrentItem;

            if (currentItem == null)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                foreach (Game game in API.Instance.Database.Games.Where(g =>
                    !(Plugin.Settings.Settings.IgnoreHiddenGamesInGameCount && g.Hidden) && (
                        (currentItem.Type == FieldType.Category && (g.CategoryIds?.Contains(currentItem.Id) ?? false)) ||
                        (currentItem.Type == FieldType.Feature && (g.FeatureIds?.Contains(currentItem.Id) ?? false)) ||
                        (currentItem.Type == FieldType.Genre && (g.GenreIds?.Contains(currentItem.Id) ?? false)) ||
                        (currentItem.Type == FieldType.Series && (g.SeriesIds?.Contains(currentItem.Id) ?? false)) ||
                        (currentItem.Type == FieldType.Tag && (g.TagIds?.Contains(currentItem.Id) ?? false)))
                ).OrderBy(g => string.IsNullOrEmpty(g.SortingName) ? g.Name : g.SortingName).ToList())
                {
                    Games.Add(new MyGame
                    {
                        Game = game,
                        Platforms = string.Join(", ", game.Platforms?.Select(x => x.Name).ToList() ?? new List<string>())
                    });
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public void CalculateItemCount()
        {
            CategoryCount = API.Instance.Database.Categories?.Count ?? 0;
            FeatureCount = API.Instance.Database.Features?.Count ?? 0;
            GenreCount = API.Instance.Database.Genres?.Count ?? 0;
            SeriesCount = API.Instance.Database.Series?.Count ?? 0;
            TagCount = API.Instance.Database.Tags?.Count ?? 0;
        }

        private bool Filter(object item) =>
            item is MetadataObject metadataObject &&
            (!GroupMatches || metadataObject.ShowGrouped) &&
            metadataObject.Name.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase) &&
            _filterTypes.Contains(metadataObject.Type) &&
            (_filterPrefix == string.Empty || metadataObject.Prefix.Equals(_filterPrefix));
    }
}