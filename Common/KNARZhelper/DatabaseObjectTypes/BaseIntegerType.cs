using KNARZhelper.Enum;
using Playnite.SDK.Models;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseIntegerType : IMetadataFieldType, INumberType, IValueType, IClearAbleType
    {
        public bool CanBeAdded => false;
        public virtual bool CanBeClearedInGame => true;
        public bool CanBeDeleted => false;
        public virtual bool CanBeEmptyInGame => true;
        public bool CanBeModified => false;
        public virtual bool CanBeSetByMetadataAddOn => true;
        public bool CanBeSetInGame => true;
        public string LabelPlural => LabelSingular;
        public abstract string LabelSingular { get; }
        public abstract FieldType Type { get; }
        public ItemValueType ValueType => ItemValueType.Integer;

        public bool AddValueToGame<T>(Game game, T value) => AddValueToGame(game, value as int?);

        public abstract bool AddValueToGame(Game game, int? value);

        public abstract void EmptyFieldInGame(Game game);

        public abstract bool FieldInGameIsEmpty(Game game);

        public bool GameContainsValue<T>(Game game, T value) => value is int intValue && GetValue(game) == (ulong)intValue;

        public abstract ulong? GetValue(Game game);

        public bool IsBiggerThan<T>(Game game, T value) => value is int intValue && GetValue(game) > (ulong)intValue;

        public bool IsSmallerThan<T>(Game game, T value) => value is int intValue && GetValue(game) < (ulong)intValue;
    }
}