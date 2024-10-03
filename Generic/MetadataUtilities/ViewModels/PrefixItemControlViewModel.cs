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
        private readonly MetadataUtilities _plugin;
        private Game _game;
        private Guid _gameId = Guid.Empty;
        private ObservableCollection<PrefixItemList> _itemLists = new ObservableCollection<PrefixItemList>();

        public PrefixItemControlViewModel(MetadataUtilities plugin) => _plugin = plugin;

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
            _game = API.Instance.Database.Games[GameId];

            ItemLists.Clear();

            foreach (var itemList in _plugin.Settings.Settings.PrefixItemTypes.Where(x => x.FieldType != FieldType.Empty && x.Name != default))
            {
                var tempList = new PrefixItemList(_plugin, _game, itemList);

                if (tempList.Items.Count > 0)
                {
                    ItemLists.Add(new PrefixItemList(_plugin, _game, itemList));
                }
            }
        }
    }
}
