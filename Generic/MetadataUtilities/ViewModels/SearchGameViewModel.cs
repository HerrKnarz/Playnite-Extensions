﻿using MetadataUtilities.Models;
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

namespace MetadataUtilities.ViewModels
{
    public class SearchGameViewModel : ObservableObject, IEditableObject
    {
        private readonly List<MetadataObject> _metadataObjects;
        private FilterPreset _currentPreset;
        private ObservableCollection<FilterPreset> _filterPresets;
        private ObservableCollection<MyGame> _games = new ObservableCollection<MyGame>();
        private CollectionViewSource _gamesViewSource;
        private MetadataUtilities _plugin;
        private string _searchTerm = string.Empty;

        public SearchGameViewModel(MetadataUtilities plugin, List<MetadataObject> metadataObjects)
        {
            Plugin = plugin;
            _metadataObjects = metadataObjects;

            _filterPresets = API.Instance.Database.FilterPresets.OrderBy(x => x.Name).ToObservable();

            GamesViewSource = new CollectionViewSource
            {
                Source = _games
            };

            GamesViewSource.SortDescriptions.Add(new SortDescription("RealSortingName", ListSortDirection.Ascending));
            GamesViewSource.IsLiveSortingRequested = true;
        }

        public RelayCommand<IList<object>> AddGamesCommand => new RelayCommand<IList<object>>(items =>
        {
            if (_metadataObjects == null || _metadataObjects.Count == 0)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var games = items.Cast<MyGame>().Select(x => x.Game).ToList();

                if (games.Count == 0)
                {
                    return;
                }

                var gamesAffected = new List<Game>();

                foreach (var game in games
                             .Select(game => new
                             {
                                 game,
                                 mustUpdate =
                                     _metadataObjects.Aggregate(false,
                                         (current, item) => current | item.AddToGame(game))
                             })
                             .Where(t => t.mustUpdate)
                             .Select(t => t.game))
                {
                    gamesAffected.AddMissing(game);
                }

                MetadataFunctions.UpdateGames(gamesAffected, Plugin.Settings.Settings);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }, items => items != null && items.Count > 0);

        public RelayCommand<Window> CloseCommand => new RelayCommand<Window>(win =>
                {
                    Plugin.Settings.Settings.GameSearchWindowHeight = Convert.ToInt32(win.Height);
                    Plugin.Settings.Settings.GameSearchWindowWidth = Convert.ToInt32(win.Width);
                    Plugin.SavePluginSettings(Plugin.Settings.Settings);

                    win.DialogResult = true;
                    win.Close();
                });

        public FilterPreset CurrentPreset
        {
            get => _currentPreset;
            set
            {
                SetValue(ref _currentPreset, value);
                LoadGames();
            }
        }

        public ObservableCollection<FilterPreset> FilterPresets
        {
            get => _filterPresets;
            set => SetValue(ref _filterPresets, value);
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

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set => SetValue(ref _plugin, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                SetValue(ref _searchTerm, value);
                LoadGames();
            }
        }

        public void BeginEdit()
        {
        }

        public void CancelEdit()
        {
        }

        public void EndEdit()
        {
        }

        private void LoadGames()
        {
            Games.Clear();

            if (_searchTerm.Length == 0)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                FilterPresetSettings filterSettings;

                if (_currentPreset != null)
                {
                    filterSettings = _currentPreset.Settings;
                    filterSettings.Name = _searchTerm;
                }
                else
                {
                    filterSettings = new FilterPresetSettings
                    {
                        Name = _searchTerm
                    };
                }

                var games = API.Instance.Database.GetFilteredGames(filterSettings, true);

                foreach (var game in games.OrderBy(g => string.IsNullOrEmpty(g.SortingName) ? g.Name : g.SortingName).ToList())
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
    }
}