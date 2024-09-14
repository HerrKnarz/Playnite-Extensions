using KNARZhelper.Enum;
using Playnite.SDK.Models;
using System;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseDateType : IMetadataFieldType, INumberType
    {
        public bool CanBeAdded => false;
        public virtual bool CanBeClearedInGame => false;
        public bool CanBeDeleted => false;
        public virtual bool CanBeEmptyInGame => false;
        public bool CanBeModified => false;
        public virtual bool CanBeSetByMetadataAddOn => false;
        public virtual bool CanBeSetInGame => false;
        public string LabelPlural => LabelSingular;
        public abstract string LabelSingular { get; }
        public abstract FieldType Type { get; }
        public ItemValueType ValueType => ItemValueType.Date;

        public abstract bool FieldInGameIsEmpty(Game game);

        public bool IsBiggerThan<T>(Game game, T value) => value is DateTime && IsBiggerThan(game, value);

        public abstract bool IsBiggerThan(Game game, DateTime? value);

        public bool IsSmallerThan<T>(Game game, T value) => value is DateTime && IsSmallerThan(game, value);

        public abstract bool IsSmallerThan(Game game, DateTime? value);
    }
}