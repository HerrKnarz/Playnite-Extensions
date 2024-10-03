using KNARZhelper;
using KNARZhelper.Enum;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;

// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable PossibleNullReferenceException

namespace MetadataUtilities.ViewModels
{
    public class MetadataEditorViewModel : ObservableObject, IEditableObject
    {
        private readonly bool _showRelatedGames;
        private MetadataObjects _completeMetadata;
        private bool _filterHideUnused;
        private string _filterPrefix = string.Empty;
        private ObservableCollection<FilterType> _filterTypes;
        private ObservableCollection<MyGame> _games = new ObservableCollection<MyGame>();
        private CollectionViewSource _gamesViewSource;
        private bool _groupMatches;
        private CollectionViewSource _metadataViewSource;
        private MetadataUtilities _plugin;
        private string _searchTerm = string.Empty;
        private List<MetadataObject> _selectedItems;

        public MetadataEditorViewModel(MetadataUtilities plugin, MetadataObjects objects)
        {
            if (plugin.Settings.Settings.WriteDebugLog)
            {
                Log.Debug("=== MetadataEditorViewModel: Start ===");
            }

            var ts = DateTime.Now;

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

                if (plugin.Settings.Settings.WriteDebugLog)
                {
                    Log.Debug($"=== MetadataEditorViewModel: Start MetadataViewSource ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                    ts = DateTime.Now;
                }

                MetadataViewSource = new CollectionViewSource
                {
                    Source = _completeMetadata
                };

                if (plugin.Settings.Settings.WriteDebugLog)
                {
                    Log.Debug($"=== MetadataEditorViewModel: Source set ({_completeMetadata.Count} rows, {(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                    ts = DateTime.Now;
                }

                using (MetadataViewSource.DeferRefresh())
                {
                    MetadataViewSource.SortDescriptions.Add(new SortDescription("TypeLabel", ListSortDirection.Ascending));
                    MetadataViewSource.SortDescriptions.Add(new SortDescription("Prefix", ListSortDirection.Ascending));
                    MetadataViewSource.SortDescriptions.Add(new SortDescription("EditName", ListSortDirection.Ascending));
                    MetadataViewSource.IsLiveSortingRequested = true;

                    if (plugin.Settings.Settings.WriteDebugLog)
                    {
                        Log.Debug($"=== MetadataEditorViewModel: Sort set ({_completeMetadata.Count} rows, {(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                        ts = DateTime.Now;
                    }

                    // We copy the settings, so we won't overwrite them when closing the window
                    // without using the close command
                    _filterTypes = plugin.Settings.Settings.FilterTypes
                        .Select(x => new FilterType() { Selected = x.Selected, Type = x.Type }).ToObservable();

                    foreach (var filterType in FilterTypes)
                    {
                        filterType.PropertyChanged += (x, y) =>
                        {
                            ((IEditableCollectionView)MetadataViewSource.View).CommitEdit();
                            MetadataViewSource.View.Filter = Filter;
                        };

                        filterType.UpdateCount();
                    }
                }

                _showRelatedGames = true;

                MetadataViewSource.View.Filter = Filter;

                if (plugin.Settings.Settings.WriteDebugLog)
                {
                    Log.Debug($"=== MetadataEditorViewModel: Filter set ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                    ts = DateTime.Now;
                }

                MetadataViewSource.View.MoveCurrentToFirst();

                if (plugin.Settings.Settings.WriteDebugLog)
                {
                    Log.Debug($"=== MetadataEditorViewModel: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                }
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
                var type = FieldType.Tag;
                var prefix = string.Empty;

                if (MetadataViewSource.View.CurrentItem != null)
                {
                    var templateItem = (MetadataObject)MetadataViewSource.View.CurrentItem;

                    type = templateItem.Type;
                    prefix = templateItem.Prefix;
                }

                var newItem = CompleteMetadata.AddNewItem(type, prefix, true, true);

                if (newItem == null)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                UpdateGroupDisplay(CompleteMetadata.ToList());

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

        public RelayCommand AddNewGameCommand => new RelayCommand(() =>
        {
            if (SelectedItems == null)
            {
                return;
            }

            var viewModel = new SearchGameViewModel(Plugin, SelectedItems);

            var searchGameView = new SearchGameView();

            var window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCSearchLabel"),
                Plugin.Settings.Settings.GameSearchWindowWidth, Plugin.Settings.Settings.GameSearchWindowHeight);
            window.Content = searchGameView;
            window.DataContext = viewModel;

            window.ShowDialog();
            LoadRelatedGames();

            foreach (var item in SelectedItems)
            {
                item.GetGameCount();
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

                var changeItems = new MetadataObjects(Plugin.Settings.Settings);

                changeItems.AddMissing(items.ToList().Cast<MetadataObject>());

                var viewModel = new ChangeTypeViewModel(Plugin, changeItems);

                var view = new ChangeTypeView();

                var window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesDialogChangeType"));
                window.Content = view;
                window.DataContext = viewModel;

                if (!(window.ShowDialog() ?? false))
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                foreach (var itemToRemove in changeItems)
                {
                    if (!itemToRemove.ExistsInDb())
                    {
                        CompleteMetadata.Remove(itemToRemove);
                    }
                    else
                    {
                        itemToRemove.GetGameCount();
                    }
                }

                foreach (var itemToAdd in viewModel.NewObjects)
                {
                    itemToAdd.GetGameCount();
                }

                CompleteMetadata.AddMissing(viewModel.NewObjects);

                UpdateGroupDisplay(CompleteMetadata.ToList());
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

        public RelayCommand<Window> CloseCommand => new RelayCommand<Window>(win =>
        {
            Plugin.Settings.Settings.FilterTypes = FilterTypes;
            Plugin.Settings.Settings.EditorWindowHeight = Convert.ToInt32(win.Height);
            Plugin.Settings.Settings.EditorWindowWidth = Convert.ToInt32(win.Width);
            Plugin.SavePluginSettings(Plugin.Settings.Settings);

            win.DialogResult = true;
            win.Close();
        });

        public MetadataObjects CompleteMetadata
        {
            get => _completeMetadata;
            set => SetValue(ref _completeMetadata, value);
        }

        public bool FilterHideUnused
        {
            get => _filterHideUnused;
            set
            {
                SetValue(ref _filterHideUnused, value);

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

        public ObservableCollection<FilterType> FilterTypes
        {
            get => _filterTypes;
            set => SetValue(ref _filterTypes, value);
        }

        public Visibility GameGridCompletionStatusVisibility => _plugin.Settings.Settings.GameGridShowCompletionStatus
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility GameGridHiddenVisibility => _plugin.Settings.Settings.GameGridShowHidden
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
                        UpdateGroupDisplay(CompleteMetadata.ToList());
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

                var mergeItems = new MetadataObjects(Plugin.Settings.Settings);

                mergeItems.AddMissing(items.ToList().Cast<MetadataObject>());

                var viewModel = new MergeDialogViewModel(Plugin, mergeItems);

                var mergeView = new MergeDialogView();

                var window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesEditorMerge"));
                window.Content = mergeView;
                window.DataContext = viewModel;

                if (!(window.ShowDialog() ?? false))
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                foreach (var itemToRemove in mergeItems)
                {
                    if (!itemToRemove.ExistsInDb())
                    {
                        CompleteMetadata.Remove(itemToRemove);
                    }
                    else
                    {
                        itemToRemove.GetGameCount();
                    }
                }

                UpdateGroupDisplay(CompleteMetadata.ToList());

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

        public RelayCommand<IList<object>> MergeRenameCommand => new RelayCommand<IList<object>>(items =>
        {
            if (items == null || items.Count > 1 || MetadataViewSource.View.CurrentItem == null)
            {
                return;
            }

            try
            {
                // We prepare the original item, save the old values and create a new one to edit.
                var templateItem = (MetadataObject)MetadataViewSource.View.CurrentItem;

                var oldItem = new MetadataObject(Plugin.Settings.Settings)
                {
                    Type = templateItem.Type,
                    Name = templateItem.Name
                };

                var newItem = new MetadataObject(Plugin.Settings.Settings)
                {
                    Type = templateItem.Type,
                    Name = templateItem.Name
                };

                // Using the Add New dialog we rename the item
                var window = AddNewObjectViewModel.GetWindow(Plugin.Settings.Settings, newItem, false,
                    ResourceProvider.GetString("LOCMetadataUtilitiesEditorMergeRename"));

                // return if the dialog was canceled or the name remained the same.
                if (window == null || !(window.ShowDialog() ?? false) || oldItem.Name == newItem.Name)
                {
                    return;
                }

                // Now we try to actually rename the item in the data grid and return, if that
                // wasn't successful. An error message already was displayed in that case.
                templateItem.EditName = newItem.EditName;

                if (templateItem.Name == oldItem.Name)
                {
                    return;
                }

                // Finally we create a new merge rule and add it to the list.
                var newRule = new MergeRule(Plugin.Settings.Settings)
                {
                    Name = newItem.Name,
                    Type = newItem.Type,
                    SourceObjects = new MetadataObjects(Plugin.Settings.Settings) { oldItem }
                };

                Plugin.Settings.Settings.MergeRules.AddRule(newRule);
                Plugin.SavePluginSettings(_plugin.Settings.Settings);

                Cursor.Current = Cursors.WaitCursor;

                UpdateGroupDisplay(CompleteMetadata.ToList());
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing rename dialog", true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }, items => items?.Count == 1);

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

        public RelayCommand<IList<object>> RemoveGamesCommand => new RelayCommand<IList<object>>(items =>
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                if (SelectedItems == null || SelectedItems.Count == 0)
                {
                    return;
                }

                var games = items.Cast<MyGame>().Select(x => x.Game).ToList();

                if (games.Count == 0)
                {
                    return;
                }

                var gamesAffected = new List<Guid>();

                foreach (var item in SelectedItems)
                {
                    gamesAffected.AddMissing(item.ReplaceInDb(games).ToList());
                }

                if (gamesAffected.Count == 0)
                {
                    return;
                }

                MetadataFunctions.UpdateGames(gamesAffected, Plugin.Settings.Settings);

                LoadRelatedGames();

                foreach (var item in SelectedItems)
                {
                    item.GetGameCount();
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }, items => items != null && items.Count > 0);

        public RelayCommand<IList<object>> RemoveItemsCommand => new RelayCommand<IList<object>>(items =>
        {
            if (items == null || items.Count < 1)
            {
                return;
            }

            var response = MessageBoxResult.Yes;

            if (!Plugin.Settings.Settings.AddRemovedToUnwanted)
            {
                response = API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAddToUnwanted"), ResourceProvider.GetString("LOCMetadataUtilitiesName"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            }

            if (response == MessageBoxResult.Cancel)
            {
                return;
            }

            Plugin.IsUpdating = true;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                using (API.Instance.Database.BufferedUpdate())
                {
                    var globalProgressOptions = new GlobalProgressOptions(
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

                            foreach (var item in items.Cast<MetadataObject>())
                            {
                                item.RemoveFromDb();

                                activateGlobalProgress.CurrentProgressValue++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }, globalProgressOptions);
                }

                var unwantedItems = new List<MetadataObject>();

                foreach (var item in items.ToList().Cast<MetadataObject>())
                {
                    unwantedItems.Add(item);

                    CompleteMetadata.Remove(item);
                }

                if (response == MessageBoxResult.Yes)
                {
                    Plugin.Settings.Settings.UnwantedItems.AddItems(unwantedItems);
                }

                UpdateGroupDisplay(CompleteMetadata.ToList());
                CalculateItemCount();
            }
            finally
            {
                Plugin.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }, items => items != null && items.Count > 0);

        public RelayCommand RemoveUnusedCommand => new RelayCommand(() =>
        {
            Cursor.Current = Cursors.WaitCursor;
            Plugin.IsUpdating = true;

            try
            {
                var removedItems = MetadataFunctions.RemoveUnusedMetadata(_plugin.Settings.Settings);

                if (removedItems.Count == 0)
                {
                    return;
                }

                var itemsToRemove = CompleteMetadata
                    .Where(x => removedItems.Any(y => x.Type == y.Type && x.Name == y.Name)).ToList();

                CalculateItemCount();
                foreach (var itemToRemove in itemsToRemove)
                {
                    CompleteMetadata.Remove(itemToRemove);
                }

                UpdateGroupDisplay(CompleteMetadata.ToList());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                Plugin.IsUpdating = false;
            }
        });

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

        public List<MetadataObject> SelectedItems
        {
            get => _selectedItems;
            set
            {
                SetValue(ref _selectedItems, value);
                LoadRelatedGames();
            }
        }

        public void BeginEdit()
        { }

        public void CancelEdit()
        { }

        public void EndEdit()
        { }

        public static void ShowEditor(MetadataUtilities plugin, List<Game> games = null)
        {
            if (plugin.Settings.Settings.WriteDebugLog)
            {
                Log.Debug("=== ShowEditor: Start ===");
            }

            var ts = DateTime.Now;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var metadataObjects = new MetadataObjects(plugin.Settings.Settings);
                var windowTitle = "LOCMetadataUtilitiesEditor";

                if (games != null)
                {
                    metadataObjects.LoadGameMetadata(games);
                    windowTitle = "LOCMetadataUtilitiesEditorForGames";
                }
                else
                {
                    metadataObjects.LoadMetadata();
                }

                var viewModel = new MetadataEditorViewModel(plugin, metadataObjects);

                var editorView = new MetadataEditorView();

                var window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString(windowTitle),
                    plugin.Settings.Settings.EditorWindowWidth, plugin.Settings.Settings.EditorWindowHeight);

                window.Content = editorView;
                window.DataContext = viewModel;

                if (plugin.Settings.Settings.WriteDebugLog)
                {
                    Log.Debug($"=== ShowEditor: Show Dialog ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                }

                window.ShowDialog();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing Metadata Editor", true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public void CalculateItemCount()
        {
            if (FilterTypes == null)
            {
                return;
            }

            foreach (var type in FilterTypes)
            {
                type.UpdateCount();
            }
        }

        private bool Filter(object item) =>
            item is MetadataObject metadataObject &&
            (!GroupMatches || metadataObject.ShowGrouped) &&
            metadataObject.Name.RegExIsMatch(SearchTerm) &&
            FilterTypes.Any(x => x.Selected && x.Type == metadataObject.Type) &&
            (_filterPrefix == string.Empty || metadataObject.Prefix.Equals(_filterPrefix)) &&
            (!_filterHideUnused || metadataObject.GameCount > 0);

        private void LoadRelatedGames()
        {
            if (!_showRelatedGames)
            {
                return;
            }

            Games.Clear();

            if (SelectedItems?.Count == 0)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                foreach (var game in API.Instance.Database.Games.Where(g => !g.Hidden || !Plugin.Settings.Settings.IgnoreHiddenGamesInGameCount).OrderBy(g => string.IsNullOrEmpty(g.SortingName) ? g.Name : g.SortingName))
                {
                    var addGame = SelectedItems?.All(item => item.IsInGame(game)) ?? false;

                    if (addGame)
                    {
                        Games.Add(new MyGame
                        {
                            Game = game,
                            Platforms = string.Join(", ",
                                game.Platforms?.Select(x => x.Name).ToList() ?? new List<string>())
                        });
                    }
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void UpdateGroupDisplay(List<MetadataObject> itemList)
        {
            if (_plugin.Settings.Settings.WriteDebugLog)
            {
                Log.Debug("=== UpdateGroupDisplay: Start ===");
            }

            var ts = DateTime.Now;

            var opts = new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0)) };

            Parallel.ForEach(itemList, opts, item => item.CheckGroup(itemList));

            if (_plugin.Settings.Settings.WriteDebugLog)
            {
                Log.Debug($"=== UpdateGroupDisplay: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
            }
        }
    }
}