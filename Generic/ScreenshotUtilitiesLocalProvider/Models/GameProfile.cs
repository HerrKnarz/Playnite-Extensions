using KNARZhelper;
using KNARZhelper.GamesCommon;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace ScreenshotUtilitiesLocalProvider.Models
{
    public class GameProfile : ObservableObject
    {
        private ObservableCollection<FolderConfig> _folderConfigs = new ObservableCollection<FolderConfig>();
        private Game _game;
        private Guid _gameId;
        private bool _overrideGlobalConfigs = false;

        public GameProfile()
        { }

        public GameProfile(Guid gameId)
        {
            GameId = gameId;
        }

        public ObservableCollection<FolderConfig> FolderConfigs
        {
            get => _folderConfigs;
            set => SetValue(ref _folderConfigs, value);
        }

        [DontSerialize]
        public Game Game
        {
            get => _game;

            set => SetValue(ref _game, value);
        }

        public Guid GameId
        {
            get => _gameId;
            set
            {
                SetValue(ref _gameId, value);

                SetGame(_gameId);
            }
        }

        [DontSerialize]
        public Visibility HiddenOnDefault => GameId == default ? Visibility.Collapsed : Visibility.Visible;

        public bool OverrideGlobalConfigs
        {
            get => _overrideGlobalConfigs;
            set => SetValue(ref _overrideGlobalConfigs, value);
        }

        public void PrepareProfile(StringExpander stringExpander, Guid gameId)
        {
            FolderConfigs?.ForEach(c => c.StringExpander = stringExpander);
            GameId = gameId;
        }

        public void SetGame(Guid gameId)
        {
            Game = gameId == default ? new Game(ResourceProvider.GetString("LOCScreenshotUtilitiesLocalProviderSettingsGlobalConfig")) : API.Instance.Database.Games.Get(gameId);

            if (Game == null)
            {
                return;
            }

            FolderConfigs.ForEach(c => c.TestGame = new GameEx(Game));
        }
    }
}
