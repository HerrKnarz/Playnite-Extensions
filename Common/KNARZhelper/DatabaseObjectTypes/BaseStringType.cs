using KNARZhelper.Enum;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseStringType : IMetadataFieldType, IValueType, IClearAbleType
    {
        public bool CanBeAdded => false;
        public virtual bool CanBeClearedInGame => true;
        public bool CanBeDeleted => false;
        public bool CanBeEmptyInGame => true;
        public bool CanBeModified => false;
        public virtual bool CanBeSetByMetadataAddOn => true;
        public bool CanBeSetInGame => false;
        public bool IsList => false;
        public string LabelPlural => LabelSingular;
        public abstract string LabelSingular { get; }
        public abstract FieldType Type { get; }
        public ItemValueType ValueType => ItemValueType.String;

        public bool AddValueToGame<T>(Game game, T value) => false;

        public abstract void EmptyFieldInGame(Game game);

        public abstract bool FieldInGameIsEmpty(Game game);

        public bool GameContainsValue<T>(Game game, T value) => value is string stringValue && GameContainsValue(game, stringValue);

        public abstract bool GameContainsValue(Game game, string value);
    }
}