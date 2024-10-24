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
        private string _defaultIcon;
        private Game _game;
        private Guid _gameId = Guid.Empty;
        private ObservableCollection<PrefixItemList> _itemLists = new ObservableCollection<PrefixItemList>();

        public PrefixItemControlViewModel(FieldType type) => _fieldType = type;

        public Visibility AddButtonVisibility => ControlCenter.Instance.Settings.PrefixControlDisplayAddButton
            ? Visibility.Visible
            : Visibility.Collapsed;

        public RelayCommand<object> AddItemCommand => new RelayCommand<object>(item =>
        {
            if (!(item is PrefixItemList itemList) || _game == null)
            {
                return;
            }

            var items = MetadataFunctions.GetItemsFromAddDialog(itemList.FieldType, itemList.Prefix, false);

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
                    MetadataFunctions.UpdateGames(new List<Game> { _game });
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

        public Visibility DeleteButtonVisibility => ControlCenter.Instance.Settings.PrefixControlDisplayDeleteButton
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
            if (ControlCenter.Instance.Settings.PrefixControlConfirmDeletion &&
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
                MetadataFunctions.UpdateGames(new List<Game> { _game });
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
                case FieldType.Developer:
                    preset.Settings.Developer = filterField;
                    break;
                case FieldType.Feature:
                    preset.Settings.Feature = filterField;
                    break;
                case FieldType.Genre:
                    preset.Settings.Genre = filterField;
                    break;
                case FieldType.Platform:
                    preset.Settings.Platform = filterField;
                    break;
                case FieldType.Publisher:
                    preset.Settings.Publisher = filterField;
                    break;
                case FieldType.Region:
                    preset.Settings.Region = filterField;
                    break;
                case FieldType.Series:
                    preset.Settings.Series = filterField;
                    break;
                case FieldType.Tag:
                    preset.Settings.Tag = filterField;
                    break;
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

            foreach (var itemList in ControlCenter.Instance.Settings.PrefixItemTypes.Where(x =>
                         x.FieldType == _fieldType && x.Name != default))
            {
                ItemLists.Add(new PrefixItemList(_game, itemList));
            }

            ItemLists.Add(new PrefixItemList(_game, _fieldType, _defaultIcon));

            ItemLists = ItemLists.Where(x => x.Items.Count > 0).OrderBy(x => x.Position).ThenBy(x => x.Name)
                .ToObservable();
        }
    }
}
