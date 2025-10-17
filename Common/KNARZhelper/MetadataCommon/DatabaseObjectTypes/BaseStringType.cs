using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK.Models;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    /// <summary>
    /// Base type for string metadata fields.
    /// </summary>
    public abstract class BaseStringType : IMetadataFieldType, IValueType, IClearAbleType
    {
        public bool CanBeAdded => false;
        public virtual bool CanBeClearedInGame => true;
        public bool CanBeDeleted => false;
        public bool CanBeEmptyInGame => true;
        public bool CanBeModified => false;
        public virtual bool CanBeSetByMetadataAddOn => true;
        public bool CanBeSetInGame => true;
        public virtual bool IsDefaultToCopy => true;
        public string LabelPlural => LabelSingular;
        public abstract string LabelSingular { get; }
        public abstract FieldType Type { get; }
        public ItemValueType ValueType => ItemValueType.String;

        public bool AddValueToGame<T>(Game game, T value) => AddValueToGame(game, value as string);

        public abstract bool AddValueToGame(Game game, string value);

        public abstract void EmptyFieldInGame(Game game);

        public abstract bool FieldInGameIsEmpty(Game game);

        public bool GameContainsValue<T>(Game game, T value) => value is string stringValue && GameContainsValue(game, stringValue);

        /// <summary>
        /// Gets the string value of the field for the specified game.
        /// </summary>
        /// <param name="game">Game to get the value from</param>
        /// <returns>Retrieved string value</returns>
        public abstract string GetValue(Game game);

        /// <summary>
        /// Checks if the field in the specified game contains the specified string value.
        /// </summary>
        /// <param name="game">Game to check the value in</param>
        /// <param name="value">Value to check</param>
        /// <returns>True if the game contains the value, otherwise false</returns>
        public abstract bool GameContainsValue(Game game, string value);

        public bool CopyValueToGame(Game sourceGame, Game targetGame, bool replaceValue = false, bool onlyIfEmpty = false) => sourceGame != null
                && targetGame != null
                && (replaceValue || (onlyIfEmpty && FieldInGameIsEmpty(targetGame)))
                && AddValueToGame(targetGame, GetValue(sourceGame));
    }
}