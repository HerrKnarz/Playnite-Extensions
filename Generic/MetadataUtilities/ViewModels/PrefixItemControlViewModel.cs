using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MetadataUtilities.ViewModels
{
    public class PrefixItemControlViewModel : ObservableObject
    {
        private readonly FieldType _fieldType;
        private readonly MetadataUtilities _plugin;
        private string _defaultIcon;
        private Game _game;
        private Guid _gameId = Guid.Empty;
        private ObservableCollection<PrefixItemList> _itemLists = new ObservableCollection<PrefixItemList>();

        public PrefixItemControlViewModel(MetadataUtilities plugin, FieldType type)
        {
            _plugin = plugin;
            _fieldType = type;
        }

        public Visibility AddButtonVisibility => _plugin.Settings.Settings.PrefixControlDisplayAddButton
            ? Visibility.Visible
            : Visibility.Collapsed;

        public RelayCommand<object> AddItemCommand => new RelayCommand<object>(item =>
        {
            if (!(item is PrefixItemList itemList) || _game == null)
            {
                return;
            }

            var items = MetadataFunctions.GetItemsFromAddDialog(itemList.FieldType, _plugin.Settings.Settings, itemList.Prefix, false);

            if (items.Count == 0)
            {
                return;
            }

            var refreshNeeded = false;

            if (itemList.FieldType.GetTypeManager() is IEditableObjectType type)
            {
                if (type.AddValueToGame(_game, items.Select(x => x.Id).ToList()))
                {
                    refreshNeeded = true;
                    MetadataFunctions.UpdateGames(new List<Game> { _game }, _plugin.Settings.Settings);
                }
            }

            if (refreshNeeded)
            {
                RefreshData();
            }
        });

        public string DefaultIcon
        {
            get => _defaultIcon;
            set => SetValue(ref _defaultIcon, value);
        }

        public Visibility DeleteButtonVisibility => _plugin.Settings.Settings.PrefixControlDisplayDeleteButton
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Guid GameId
        {
            get => _gameId;
            set
            {
                SetValue(ref _gameId, value);

                RefreshData();
            }
        }

        public ObservableCollection<PrefixItemList> ItemLists
        {
            get => _itemLists;
            set => SetValue(ref _itemLists, value);
        }

        public RelayCommand<object> RemoveItemCommand => new RelayCommand<object>(item =>
        {
            if (_plugin.Settings.Settings.PrefixControlConfirmDeletion &&
                API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCAskRemoveItemMessage"),
                    ResourceProvider.GetString("LOCAskRemoveItemTitle"),
                    MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            if (!(item is MetadataObject metadataItem))
            {
                return;
            }

            if (!(metadataItem.TypeManager is IEditableObjectType type))
            {
                return;
            }

            var refreshNeeded = false;

            if (type.RemoveObjectFromGame(_game, metadataItem.Id))
            {
                refreshNeeded = true;
                MetadataFunctions.UpdateGames(new List<Game> { _game }, _plugin.Settings.Settings);
            }

            if (refreshNeeded)
            {
                RefreshData();
            }
        });

        public RelayCommand<object> SetFilterCommand => new RelayCommand<object>(item =>
        {
            if (!(item is MetadataObject metadataItem))
            {
                return;
            }

            var filterField = new IdItemFilterItemProperties(metadataItem.Id);

            var preset = new FilterPreset
            {
                Settings = API.Instance.MainView.GetCurrentFilterSettings()
            };

            switch (metadataItem.Type)
            {
                case FieldType.AgeRating:
                    preset.Settings.AgeRating = filterField;
                    break;
                case FieldType.Category:
                    preset.Settings.Category = filterField;
                    break;
                case FieldType.Feature:
                    preset.Settings.Feature = filterField;
                    break;
                case FieldType.Genre:
                    preset.Settings.Genre = filterField;
                    break;
                case FieldType.Series:
                    preset.Settings.Series = filterField;
                    break;
                case FieldType.Tag:
                    preset.Settings.Tag = filterField;
                    break;
                case FieldType.Empty:
                case FieldType.Background:
                case FieldType.CompletionStatus:
                case FieldType.CommunityScore:
                case FieldType.Cover:
                case FieldType.CriticScore:
                case FieldType.DateAdded:
                case FieldType.Description:
                case FieldType.Developer:
                case FieldType.Favorite:
                case FieldType.Hdr:
                case FieldType.Hidden:
                case FieldType.Icon:
                case FieldType.InstallSize:
                case FieldType.IsInstalled:
                case FieldType.LastPlayed:
                case FieldType.Library:
                case FieldType.Name:
                case FieldType.Notes:
                case FieldType.Platform:
                case FieldType.PlayCount:
                case FieldType.Publisher:
                case FieldType.OverrideInstallState:
                case FieldType.Region:
                case FieldType.ReleaseDate:
                case FieldType.SortingName:
                case FieldType.Source:
                case FieldType.TimePlayed:
                case FieldType.UserScore:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var oldGameId = _game.Id;

            API.Instance.MainView.ApplyFilterPreset(preset);

            if (oldGameId != default && API.Instance.MainView.FilteredGames.Any(g => g.Id == oldGameId))
            {
                API.Instance.MainView.SelectGame(oldGameId);
            }
        });

        public void RefreshData()
        {
            if (GameId == Guid.Empty)
            {
                return;
            }

            _game = API.Instance.Database.Games[GameId];

            ItemLists.Clear();

            foreach (var itemList in _plugin.Settings.Settings.PrefixItemTypes.Where(x =>
                         x.FieldType == _fieldType && x.Name != default))
            {
                ItemLists.Add(new PrefixItemList(_plugin, _game, itemList));
            }

            ItemLists.Add(new PrefixItemList(_plugin, _game, _fieldType, _defaultIcon));

            ItemLists = ItemLists.Where(x => x.Items.Count > 0).OrderBy(x => x.Position).ThenBy(x => x.Name)
                .ToObservable();
        }
    }
}
