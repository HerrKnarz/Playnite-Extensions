using KNARZhelper.Enum;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseUlongType : IMetadataFieldType, INumberType, IValueType, IClearAbleType
    {
        public bool CanBeAdded => false;
        public virtual bool CanBeClearedInGame => true;
        public bool CanBeDeleted => false;
        public virtual bool CanBeEmptyInGame => true;
        public bool CanBeModified => false;
        public virtual bool CanBeSetByMetadataAddOn => true;
        public bool CanBeSetInGame => true;
        public virtual bool IsDefaultToCopy => true;
        public string LabelPlural => LabelSingular;
        public abstract string LabelSingular { get; }
        public abstract FieldType Type { get; }
        public ItemValueType ValueType => ItemValueType.Ulong;

        public bool AddValueToGame<T>(Game game, T value) => AddValueToGame(game, value as ulong?);

        public abstract bool AddValueToGame(Game game, ulong? value);

        public bool CopyValueToGame(Game sourceGame, Game targetGame, bool replaceValue = false, bool onlyIfEmpty = false) => sourceGame != null
            && targetGame != null
            && (replaceValue || (onlyIfEmpty && FieldInGameIsEmpty(targetGame)))
            && AddValueToGame(targetGame, GetValue(sourceGame));

        public abstract void EmptyFieldInGame(Game game);

        public abstract bool FieldInGameIsEmpty(Game game);

        public bool GameContainsValue<T>(Game game, T value) => value is ulong ulongValue && GetValue(game) == ulongValue;

        public abstract ulong GetValue(Game game);

        public bool IsBiggerThan<T>(Game game, T value) => value is ulong ulongValue && GetValue(game) > ulongValue;

        public bool IsSmallerThan<T>(Game game, T value) => value is ulong ulongValue && GetValue(game) < ulongValue;
    }
}
