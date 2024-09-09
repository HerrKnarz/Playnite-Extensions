using Playnite.SDK;
using System;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeDateAdded : BaseType
    {
        public override bool CanBeAdded => false;
        public override bool CanBeClearedInGame => false;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => false;
        public override bool CanBeModified => false;
        public override bool CanBeSetByMetadataAddOn => false;
        public override bool CanBeSetInGame => false;
        public override bool IsList => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCDateAddedLabel");
        public override FieldType Type => FieldType.DateAdded;
        public override ItemValueType ValueType => ItemValueType.Date;

        public override bool FieldInGameIsEmpty(Game game) => !game.Added.HasValue;

        public override bool IsBiggerThan<T>(Game game, T value) =>
            (value != null || value is DateTime) && game.Added > (value as DateTime?);

        public override bool IsSmallerThan<T>(Game game, T value) =>
            (value != null || value is DateTime) && game.Added < (value as DateTime?);
    }
}