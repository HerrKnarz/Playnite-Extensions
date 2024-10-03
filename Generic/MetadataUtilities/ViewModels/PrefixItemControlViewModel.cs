using KNARZhelper.Enum;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace MetadataUtilities.ViewModels
{
    public class PrefixItemControlViewModel : ObservableObject
    {
        private readonly MetadataUtilities _plugin;
        private Game _game;
        private Guid _gameId = Guid.Empty;
        private PrefixItemList _tags;

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

        public PrefixItemList Tags
        {
            get => _tags;
            set => SetValue(ref _tags, value);
        }

        public void RefreshData()
        {
            _game = API.Instance.Database.Games[GameId];

            Tags = new PrefixItemList(_plugin, _game, FieldType.Tag);

        }
    }
}
