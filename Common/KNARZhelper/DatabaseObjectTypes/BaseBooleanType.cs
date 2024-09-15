using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseBooleanType : IMetadataFieldType, IValueType, IGameInfoType, IClearAbleType
    {
        public bool CanBeAdded => false;
        public bool CanBeClearedInGame => true;
        public bool CanBeDeleted => false;
        public bool CanBeEmptyInGame => true;
        public bool CanBeModified => false;
        public bool CanBeSetByMetadataAddOn => false;
        public bool CanBeSetInGame => true;
        public virtual string LabelPlural => LabelSingular;
        public abstract string LabelSingular { get; }
        public abstract FieldType Type { get; }
        public ItemValueType ValueType => ItemValueType.Boolean;

        public bool AddValueToGame<T>(Game game, T value) => AddValueToGame(game, value as bool?);

        public abstract bool AddValueToGame(Game game, bool? value);

        public abstract void EmptyFieldInGame(Game game);

        public bool FieldInGameIsEmpty(Game game) => !GetValue(game);

        public int GetGameCount(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Count(g => !(ignoreHidden && g.Hidden) && GetValue(g));

        public int GetGameCount(List<Game> games, Guid id, bool ignoreHidden = false) =>
            games.Count(g => !(ignoreHidden && g.Hidden) && GetValue(g));

        public List<Game> GetGames(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHidden && g.Hidden) && GetValue(g)).ToList();

        public abstract bool GetValue(Game game);
    }
}