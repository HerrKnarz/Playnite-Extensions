﻿using KNARZhelper;
using KNARZhelper.Enum;
using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.Enum;
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

namespace MetadataUtilities.ViewModels
{
    public class MetadataEditorViewModel : ObservableObject
    {
        private readonly Settings _settings = ControlCenter.Instance.Settings;
        private MetadataObjects _completeMetadata;
        private bool _filterHideUnused;
        private string _filterPrefix = string.Empty;
        private ObservableCollection<FilterType> _filterTypes;
        private ObservableCollection<MyGame> _games = new ObservableCollection<MyGame>();
        private CollectionViewSource _gamesViewSource;
        private bool _groupMatches;
        private CollectionViewSource _metadataViewSource;
        private string _searchTerm = string.Empty;
        private List<MetadataObject> _selectedItems;

        public MetadataEditorViewModel(MetadataObjects objects)
        {
            if (_settings.WriteDebugLog)
            {
                Log.Debug("=== MetadataEditorViewModel: Start ===");
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                PrepareMetadata(objects);

                var ts = DateTime.Now;

                Prefixes.Add(string.Empty);
                Prefixes.AddMissing(_settings.Prefixes);

                if (_settings.WriteDebugLog)
                {
                    Log.Debug($"=== MetadataEditorViewModel: Added prefixex ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                    ts = DateTime.Now;
                }

                CalculateItemCount();

                if (_settings.WriteDebugLog)
                {
                    Log.Debug($"=== MetadataEditorViewModel: calculated item count ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                    ts = DateTime.Now;
                }

                PrepareGamesViewSource();

                if (_settings.WriteDebugLog)
                {
                    Log.Debug($"=== MetadataEditorViewModel: prepared games view source ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                }

                PrepareMetadataViewSource();
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

                newItem.RenameObject += OnRenameObject;

                RefreshView(newItem);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing merge dialog", true);
            }
        });

        public RelayCommand AddNewGameCommand => new RelayCommand(() =>
        {
            if (SelectedItems == null)
            {
                return;
            }

            var viewModel = new SearchGameViewModel(SelectedItems);

            var searchGameView = new SearchGameView();

            var window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCSearchLabel"),
                _settings.GameSearchWindowWidth, _settings.GameSearchWindowHeight);
            window.Content = searchGameView;
            window.DataContext = viewModel;

            window.ShowDialog();
            LoadRelatedGames();

            foreach (var item in SelectedItems)
            {
                item.GetGameCount();
            }
        });

        public RelayCommand<IList<object>> AddToWhiteListCommand => new RelayCommand<IList<object>>(items =>
        {
            if (items == null || items.Count < 1)
            {
                return;
            }

            var mustUpdate = false;

            foreach (var item in items.Cast<MetadataObject>())
            {
                if (_settings.UnusedItemsWhiteList.Any(x => x.TypeAndName == item.TypeAndName))
                {
                    continue;
                }

                _settings.UnusedItemsWhiteList.Add(new WhiteListItem(item.Type, item.Name));

                mustUpdate = true;
            }

            if (!mustUpdate)
            {
                return;
            }

            {
                _settings.UnusedItemsWhiteList.Sort(x => x.TypeAndName);

                ControlCenter.Instance.SavePluginSettings();
            }
        }, items => items != null && items.Count > 0);

        public RelayCommand<IList<object>> ChangeTypeCommand => new RelayCommand<IList<object>>(items =>
        {
            if (items == null || items.Count < 1)
            {
                return;
            }

            try
            {
                ControlCenter.Instance.IsUpdating = true;

                var changeItems = new MetadataObjects();

                changeItems.AddMissing(items.ToList().Cast<MetadataObject>());

                var viewModel = new ChangeTypeViewModel(changeItems);

                var view = new ChangeTypeView();

                var window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesDialogChangeType"));
                window.Content = view;
                window.DataContext = viewModel;

                if (!(window.ShowDialog() ?? false))
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                using (MetadataViewSource.DeferRefresh())
                {
                    foreach (var itemToRemove in changeItems)
                    {
                        if (!itemToRemove.ExistsInDb())
                        {
                            CompleteMetadata.Remove(itemToRemove);
                        }
                        else
                        {
                            itemToRemove.GetGameCount();
                            CompleteMetadata.GetSibling(itemToRemove)?.GetGameCount();
                        }
                    }

                    foreach (var itemToAdd in viewModel.NewObjects)
                    {
                        itemToAdd.GetGameCount();
                        itemToAdd.RenameObject += OnRenameObject;
                    }

                    CompleteMetadata.AddMissing(viewModel.NewObjects);

                    RefreshView();
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing change type dialog", true);
            }
            finally
            {
                ControlCenter.Instance.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }, items => items?.Count > 0);

        public RelayCommand<Window> CloseCommand => new RelayCommand<Window>(win =>
        {
            _settings.FilterTypes = FilterTypes;
            _settings.EditorWindowHeight = Convert.ToInt32(win.Height);
            _settings.EditorWindowWidth = Convert.ToInt32(win.Width);
            ControlCenter.Instance.SavePluginSettings();

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

        public Visibility GameGridCompletionStatusVisibility => _settings.GameGridShowCompletionStatus
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility GameGridHiddenVisibility => _settings.GameGridShowHidden
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility GameGridPlatformVisibility => _settings.GameGridShowPlatform
                    ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility GameGridReleaseVisibility => _settings.GameGridShowReleaseYear
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
                        UpdateGroupDisplay();
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
                ControlCenter.Instance.IsUpdating = true;

                var mergeItems = new MetadataObjects();

                mergeItems.AddMissing(items.ToList().Cast<MetadataObject>());

                var viewModel = new MergeDialogViewModel(mergeItems);

                var mergeView = new MergeDialogView();

                var window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesEditorMerge"));
                window.Content = mergeView;
                window.DataContext = viewModel;

                if (!(window.ShowDialog() ?? false))
                {
                    return;
                }

                RemoveItems(mergeItems);

                LoadRelatedGames();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing merge dialog", true);
            }
            finally
            {
                ControlCenter.Instance.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }, items => items?.Count > 1);

        public RelayCommand<IList<object>> MergeRenameCommand => new RelayCommand<IList<object>>(items =>
        {
            if (items == null || items.Count == 0 || MetadataViewSource.View.CurrentItem == null)
            {
                return;
            }

            try
            {
                ControlCenter.Instance.IsUpdating = true;

                var itemsToMerge = new MetadataObjects();

                // We clone the items, so we can add them to the merge rule while the originals get changed here.
                itemsToMerge.AddMissing(items.ToList().Cast<MetadataObject>());

                var mergeItems = itemsToMerge.DeepClone();

                // We prepare the original item, save the old values and create a new one to edit.
                var templateItem = (MetadataObject)MetadataViewSource.View.CurrentItem;

                var oldItem = new MetadataObject(templateItem.Type, templateItem.Name);

                var newItem = new MetadataObject(templateItem.Type, templateItem.Name);

                // Using the Add New dialog we rename the item
                var window = AddNewObjectViewModel.GetWindow(newItem, false,
                    ResourceProvider.GetString("LOCMetadataUtilitiesEditorMergeRename"));

                // return if the dialog was canceled or the name remained the same.
                if (window == null || !(window.ShowDialog() ?? false) || oldItem.Name == newItem.Name)
                {
                    return;
                }

                // Now we try to actually rename the item in the data grid and return, if that
                // wasn't successful. An error message already was displayed in that case.
                if (!templateItem.UpdateItem(newItem.Name))
                {
                    return;
                }

                templateItem.Name = newItem.Name;

                ControlCenter.Instance.MergeMetadataObjects(newItem, mergeItems, true);

                RemoveItems(itemsToMerge);

                RefreshView();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing rename dialog", true);
            }
            finally
            {
                ControlCenter.Instance.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }, items => items?.Count > 0);

        public void RemoveItems(MetadataObjects mergeItems)
        {
            Cursor.Current = Cursors.WaitCursor;

            using (MetadataViewSource.DeferRefresh())
            {
                foreach (var itemToRemove in mergeItems)
                {
                    if (!itemToRemove.ExistsInDb())
                    {
                        CompleteMetadata.Remove(itemToRemove, true);
                    }
                    else
                    {
                        itemToRemove.GetGameCount();

                        CompleteMetadata.GetSibling(itemToRemove)?.GetGameCount();
                    }
                }

                UpdateGroupDisplay();
                CalculateItemCount();
            }
        }

        public CollectionViewSource MetadataViewSource
        {
            get => _metadataViewSource;
            set => SetValue(ref _metadataViewSource, value);
        }

        public ObservableCollection<string> Prefixes { get; } = new ObservableCollection<string>();

        public Visibility PrefixVisibility => (_settings.Prefixes?.Count ?? 0) > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        public RelayCommand<IList<object>> LoadGameCommand => new RelayCommand<IList<object>>(items =>
        {
            if (SelectedItems == null || SelectedItems.Count == 0)
            {
                return;
            }

            var game = items.Cast<MyGame>().Select(x => x.Game).First();

            if (game == null)
            {
                return;
            }

            API.Instance.MainView.SelectGame(game.Id);
        }, items => items != null && items.Count > 0);

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

                ControlCenter.UpdateGames(gamesAffected);

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

            if (!_settings.AddRemovedToUnwanted)
            {
                response = API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAddToUnwanted"), ResourceProvider.GetString("LOCMetadataUtilitiesName"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            }

            if (response == MessageBoxResult.Cancel)
            {
                return;
            }

            ControlCenter.Instance.IsUpdating = true;
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

                var unwantedItems = new List<BaseMetadataObject>();

                using (MetadataViewSource.DeferRefresh())
                {
                    foreach (var item in items.ToList().Cast<MetadataObject>())
                    {
                        unwantedItems.Add(item);

                        CompleteMetadata.Remove(item);
                    }

                    if (response == MessageBoxResult.Yes)
                    {
                        _settings.UnwantedItems.AddItems(unwantedItems);
                    }

                    UpdateGroupDisplay();
                    CalculateItemCount();
                }
            }
            finally
            {
                ControlCenter.Instance.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }, items => items != null && items.Count > 0);

        public RelayCommand RemoveUnusedCommand => new RelayCommand(() =>
        {
            Cursor.Current = Cursors.WaitCursor;
            ControlCenter.Instance.IsUpdating = true;

            try
            {
                var removedItems = ControlCenter.Instance.RemoveUnusedMetadata();

                if (removedItems.Count == 0)
                {
                    return;
                }

                using (MetadataViewSource.DeferRefresh())
                {
                    var itemsToRemove = CompleteMetadata
                        .Where(x => removedItems.Any(y => x.Type == y.Type && x.Name == y.Name)).ToList();

                    CalculateItemCount();
                    foreach (var itemToRemove in itemsToRemove)
                    {
                        CompleteMetadata.Remove(itemToRemove);
                    }

                    UpdateGroupDisplay();
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                ControlCenter.Instance.IsUpdating = false;
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

        public static void ShowEditor(List<Game> games = null)
        {
            var settings = ControlCenter.Instance.Settings;

            if (settings.WriteDebugLog)
            {
                Log.Debug("=== ShowEditor: Start ===");
            }

            var ts = DateTime.Now;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var metadataObjects = new MetadataObjects();
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

                var viewModel = new MetadataEditorViewModel(metadataObjects);

                var editorView = new MetadataEditorView();

                var window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString(windowTitle),
                    settings.EditorWindowWidth, settings.EditorWindowHeight);

                window.Content = editorView;
                window.DataContext = viewModel;

                if (settings.WriteDebugLog)
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

        private void AddNewItem(FieldType type, string name, Guid id)
        {
            var newItem = new MetadataObject(type, name)
            {
                Id = id
            };

            newItem.RenameObject += OnRenameObject;
            newItem.GetGameCount();

            CompleteMetadata.Add(newItem);
        }

        private bool Filter(object item)
        {
            return item is MetadataObject metadataObject &&
            (!GroupMatches || metadataObject.ShowGrouped) &&
            metadataObject.Name.RegExIsMatch(SearchTerm) &&
            FilterTypes.Any(x => x.Selected && x.Type == metadataObject.Type) &&
            (_filterPrefix == string.Empty || metadataObject.Prefix.Equals(_filterPrefix)) &&
            (!_filterHideUnused || metadataObject.GameCount > 0);
        }

        private void LoadRelatedGames()
        {
            Games.Clear();

            if (SelectedItems?.Count == 0)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                foreach (var game in API.Instance.Database.Games.Where(g => !g.Hidden || !_settings.IgnoreHiddenGamesInGameCount).OrderBy(g => string.IsNullOrEmpty(g.SortingName) ? g.Name : g.SortingName))
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

        private bool OnRenameObject(object sender, string oldName, string newName)
        {
            if (!(sender is MetadataObject item))
            {
                return false;
            }

            var renameOther = false;
            var splitCompany = false;
            MetadataObject otherItem = null;
            IMetadataFieldType otherType = null;

            if (item.TypeManager is BaseCompanyType)
            {
                otherType = item.Type == FieldType.Developer ? FieldType.Publisher.GetTypeManager() : FieldType.Developer.GetTypeManager();
                otherItem = CompleteMetadata.FirstOrDefault(x => x.Type == otherType.Type && x.Name == item.Name);

                switch (API.Instance.Dialogs.ShowMessage(
                            string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogRenameCompanyQuestion"),
                                item.TypeLabel, otherType.LabelSingular),
                            ResourceProvider.GetString("LOCMetadataUtilitiesDialogRenameCompany"),
                            MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Cancel:
                        return false;
                    case MessageBoxResult.Yes:
                        renameOther = true;

                        break;
                    case MessageBoxResult.No:
                        splitCompany = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var res = item.UpdateName(newName);

            switch (res)
            {
                case DbInteractionResult.IsDuplicate:
                    API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAlreadyExists"),
                        item.TypeLabel));

                    return false;
                case DbInteractionResult.Updated:
                    {
                        // We simply rename the object in the editor, since its original was already renamed.
                        if (renameOther)
                        {
                            if (otherItem == null)
                            {
                                return true;
                            }

                            CompleteMetadata.Remove(otherItem);

                            AddNewItem(otherType.Type, newName, item.Id);

                            RefreshView();
                        }
                        // If the other company type's name is supposed to stay the same, we create a new one with the old name and change all games to it.
                        else if (splitCompany && (otherType is IEditableObjectType objectType))
                        {
                            var newId = objectType.AddDbObject(oldName);

                            var itemToDelete = new MetadataObject(objectType.Type, newName);

                            var gamesAffected = itemToDelete.ReplaceInDb(API.Instance.Database.Games.ToList(),
                            objectType.Type, newId);

                            ControlCenter.UpdateGames(gamesAffected.ToList());

                            // We now have the renamed company and the other type with the old name still. We need to remove the other
                            // and add three new ones: A renamed other type and a developer and publisher with the old name.

                            AddNewItem(item.Type, oldName, newId);

                            //If we didn't have the other item in the view, we also don't need to add a new one.
                            if (otherItem == null)
                            {
                                return true;
                            }

                            CompleteMetadata.Remove(otherItem);

                            AddNewItem(otherType.Type, oldName, newId);
                            AddNewItem(otherType.Type, newName, item.Id);

                            RefreshView();

                            ControlCenter.Instance.RenameObject(otherType, newName, oldName);
                        }

                        return true;
                    }
                default:
                    return false;
            }
        }

        private void PrepareFilterTypes()
        {
            // We copy the settings, so we won't overwrite them when closing the window
            // without using the close command
            _filterTypes = _settings.TypeConfigs.Where(x => x.Selected)
                .Select(x => new FilterType() { Selected = false, Type = x.Type }).OrderBy(x => x.Label).ToObservable();

            foreach (var filterType in FilterTypes)
            {
                filterType.PropertyChanged += (x, y) =>
                {
                    if (MetadataViewSource is null)
                    {
                        return;
                    }

                    ((IEditableCollectionView)MetadataViewSource.View).CommitEdit();
                    UpdateGroupDisplay();
                    MetadataViewSource.View.Filter = Filter;
                };

                filterType.Selected = _settings.FilterTypes.Any(x => x.Selected && x.Type == filterType.Type);

                filterType.UpdateCount();
            }
        }

        private void PrepareGamesViewSource()
        {
            GamesViewSource = new CollectionViewSource
            {
                Source = _games
            };

            GamesViewSource.SortDescriptions.Add(new SortDescription("RealSortingName", ListSortDirection.Ascending));
            GamesViewSource.IsLiveSortingRequested = true;
        }

        private void PrepareMetadata(MetadataObjects objects)
        {
            var ts = DateTime.Now;

            CompleteMetadata = objects;

            if (_settings.WriteDebugLog)
            {
                Log.Debug($"=== MetadataEditorViewModel: PrepareMetadata: Set CompleteMetadata ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                ts = DateTime.Now;
            }

            foreach (var item in CompleteMetadata)
            {
                item.RenameObject += OnRenameObject;
            }

            if (_settings.WriteDebugLog)
            {
                Log.Debug($"=== MetadataEditorViewModel: PrepareMetadata: set OnRenameObject ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
            }
        }

        private void PrepareMetadataViewSource()
        {
            var ts = DateTime.Now;

            if (_settings.WriteDebugLog)
            {
                Log.Debug($"=== MetadataEditorViewModel: Start MetadataViewSource ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
            }

            PrepareFilterTypes();

            MetadataViewSource = new CollectionViewSource
            {
                Source = _completeMetadata
            };

            using (MetadataViewSource.DeferRefresh())
            {
                MetadataViewSource.SortDescriptions.Add(new SortDescription("TypeLabel", ListSortDirection.Ascending));
                MetadataViewSource.SortDescriptions.Add(new SortDescription("Prefix", ListSortDirection.Ascending));
                MetadataViewSource.SortDescriptions.Add(new SortDescription("EditName", ListSortDirection.Ascending));
                MetadataViewSource.IsLiveSortingRequested = true;

                if (_settings.WriteDebugLog)
                {
                    Log.Debug($"=== MetadataEditorViewModel: Sort set ({_completeMetadata.Count} rows, {(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                    ts = DateTime.Now;
                }
            }

            if (_settings.WriteDebugLog)
            {
                Log.Debug($"=== MetadataEditorViewModel: Finished MetadataViewSource ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                ts = DateTime.Now;
            }

            UpdateGroupDisplay();

            MetadataViewSource.View.Filter = Filter;

            if (_settings.WriteDebugLog)
            {
                Log.Debug($"=== MetadataEditorViewModel: Filter set ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
                ts = DateTime.Now;
            }

            MetadataViewSource.View.MoveCurrentToFirst();

            if (_settings.WriteDebugLog)
            {
                Log.Debug($"=== MetadataEditorViewModel: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
            }
        }

        private void RefreshView(MetadataObject focusedItem = null)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                using (MetadataViewSource.DeferRefresh())
                {
                    UpdateGroupDisplay();

                    CalculateItemCount();
                }

                MetadataViewSource.View.Filter = Filter;

                if (focusedItem != null)
                {
                    MetadataViewSource.View.MoveCurrentTo(focusedItem);
                }

                LoadRelatedGames();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void UpdateGroupDisplay()
        {
            if (_settings.WriteDebugLog)
            {
                Log.Debug("=== UpdateGroupDisplay: Start ===");
            }

            var ts = DateTime.Now;

            using (MetadataViewSource.DeferRefresh())
            {
                var tempList = CompleteMetadata.Where(x => _filterTypes.Any(f => f.Selected && f.Type == x.Type)).ToList();

                var groups = tempList.GroupBy(x => x.CleanedUpName)
                    .ToDictionary(group => group.Key, group => group.Count());

                var opts = new ParallelOptions
                { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0)) };

                Parallel.ForEach(tempList, opts, item => item.CheckGroup(groups));
            }

            if (_settings.WriteDebugLog)
            {
                Log.Debug($"=== UpdateGroupDisplay: End ({(DateTime.Now - ts).TotalMilliseconds} ms) ===");
            }
        }
    }
}