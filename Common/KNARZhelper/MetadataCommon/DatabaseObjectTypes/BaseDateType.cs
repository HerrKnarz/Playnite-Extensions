using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK.Models;
using System;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    /// <summary>
    /// Base type for date metadata fields.
    /// </summary>
    public abstract class BaseDateType : IMetadataFieldType, IValueType, INumberType
    {
        public bool CanBeAdded => false;
        public virtual bool CanBeClearedInGame => false;
        public bool CanBeDeleted => false;
        public virtual bool CanBeEmptyInGame => false;
        public bool CanBeModified => false;
        public virtual bool CanBeSetByMetadataAddOn => false;
        public virtual bool CanBeSetInGame => false;
        public virtual bool IsDefaultToCopy => true;
        public string LabelPlural => LabelSingular;
        public abstract string LabelSingular { get; }
        public abstract FieldType Type { get; }
        public ItemValueType ValueType => ItemValueType.Date;

        public abstract bool AddValueToGame<T>(Game game, T value);

        public bool CopyValueToGame(Game sourceGame, Game targetGame, bool replaceValue = false, bool onlyIfEmpty = false) => sourceGame != null
            && targetGame != null
            && (replaceValue || (onlyIfEmpty && FieldInGameIsEmpty(targetGame)))
            && AddValueToGame(targetGame, GetValue(sourceGame));

        /// <summary>
        /// Checks if the field in the specified game is empty.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public abstract bool FieldInGameIsEmpty(Game game);

        public bool GameContainsValue<T>(Game game, T value) => value is DateTime dateValue && GetValue(game) == dateValue;

        /// <summary>
        /// Gets the date value of the field for the specified game. Can be null.
        /// </summary>
        /// <param name="game">Game to get the date from</param>
        /// <returns>Retrieved date</returns>
        public abstract DateTime? GetValue(Game game);

        public bool IsBiggerThan<T>(Game game, T value) => value is DateTime dateValue && GetValue(game) > dateValue;

        public bool IsSmallerThan<T>(Game game, T value) => value is DateTime dateValue && GetValue(game) < dateValue;
    }
}