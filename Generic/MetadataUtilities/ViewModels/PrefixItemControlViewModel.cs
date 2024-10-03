using KNARZhelper.Enum;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MetadataUtilities.ViewModels
{
    public class PrefixItemControlViewModel : ObservableObject
    {
        private readonly string _defaultIcon;
        private readonly FieldType _fieldType;
        private readonly MetadataUtilities _plugin;
        private Game _game;
        private Guid _gameId = Guid.Empty;
        private ObservableCollection<PrefixItemList> _itemLists = new ObservableCollection<PrefixItemList>();

        public PrefixItemControlViewModel(MetadataUtilities plugin, FieldType type, string icon)
        {
            _plugin = plugin;
            _fieldType = type;
            _defaultIcon = icon;
        }

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

        public void RefreshData()
        {
            if (GameId == Guid.Empty)
            {
                return;
            }

            _game = API.Instance.Database.Games[GameId];

            ItemLists.Clear();

            ItemLists.Add(new PrefixItemList(_plugin, _game, _fieldType, _defaultIcon));

            foreach (var itemList in _plugin.Settings.Settings.PrefixItemTypes.Where(x => x.FieldType == _fieldType && x.Name != default))
            {
                ItemLists.Add(new PrefixItemList(_plugin, _game, itemList));
            }

            ItemLists = ItemLists.Where(x => x.Items.Count > 0).ToObservable();
        }
    }
}
