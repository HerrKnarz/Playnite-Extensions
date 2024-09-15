using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using MetadataUtilities.Enums;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;

namespace MetadataUtilities.Models
{
    public class Condition : MetadataObject
    {
        private ComparatorType _comparator = ComparatorType.Contains;

        private DateTime? _dateValue;
        private int? _intValue;
        private string _stringValue;

        public Condition(Settings settings) : base(settings)
        {
        }

        public ComparatorType Comparator
        {
            get => _comparator;
            set => SetValue(ref _comparator, value);
        }

        public DateTime? DateValue
        {
            get => _dateValue;
            set => SetValue(ref _dateValue, value);
        }

        public int? IntValue
        {
            get => _intValue;
            set => SetValue(ref _intValue, value);
        }

        public string StringValue
        {
            get => _stringValue;
            set => SetValue(ref _stringValue, value);
        }

        [DontSerialize]
        public new string ToString
        {
            get
            {
                switch (TypeManager.ValueType)
                {
                    case ItemValueType.Boolean:
                        return $"{TypeLabel} {Comparator.GetEnumDisplayName()}";

                    case ItemValueType.Integer:
                        return $"{TypeLabel} {Comparator.GetEnumDisplayName()} {IntValue}";

                    case ItemValueType.Date:
                        return $"{TypeLabel} {Comparator.GetEnumDisplayName()} {DateValue?.ToString("yyyy-MM-dd")}";

                    case ItemValueType.String:
                        return $"{TypeLabel} {Comparator.GetEnumDisplayName()} {StringValue}";

                    case ItemValueType.ItemList:
                    case ItemValueType.Media:
                    case ItemValueType.None:
                    default:
                        return $"{TypeLabel} {Comparator.GetEnumDisplayName()} {Name}";
                }
            }
        }

        public bool IsTrue(Game game)
        {
            switch (Comparator)
            {
                case ComparatorType.Contains:
                    if (!(TypeManager is IValueType containsType))
                    {
                        return false;
                    }

                    switch (TypeManager.ValueType)
                    {
                        case ItemValueType.Boolean:
                            return containsType.GameContainsValue(game, true);

                        case ItemValueType.ItemList:
                            return ExistsInGame(game);

                        case ItemValueType.Integer:
                            return containsType.GameContainsValue(game, IntValue);

                        case ItemValueType.String:
                            return containsType.GameContainsValue(game, StringValue);

                        case ItemValueType.Date:
                            return containsType.GameContainsValue(game, DateValue);

                        case ItemValueType.Media:
                        case ItemValueType.None:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case ComparatorType.DoesNotContain:
                    if (!(TypeManager is IValueType notContainsType))
                    {
                        return false;
                    }

                    switch (TypeManager.ValueType)
                    {
                        case ItemValueType.Boolean:
                            return !notContainsType.GameContainsValue(game, true);

                        case ItemValueType.ItemList:
                            return !ExistsInGame(game);

                        case ItemValueType.Integer:
                            return !notContainsType.GameContainsValue(game, IntValue);

                        case ItemValueType.String:
                            return !notContainsType.GameContainsValue(game, StringValue);

                        case ItemValueType.Date:
                            return !notContainsType.GameContainsValue(game, DateValue);

                        case ItemValueType.Media:
                        case ItemValueType.None:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case ComparatorType.IsEmpty:
                    return TypeManager is IClearAbleType emptyType && emptyType.FieldInGameIsEmpty(game);

                case ComparatorType.IsNotEmpty:
                    return TypeManager is IClearAbleType notEmptyType && !notEmptyType.FieldInGameIsEmpty(game);

                case ComparatorType.IsBiggerThan:
                    return TypeManager is INumberType biggerType && (TypeManager.ValueType == ItemValueType.Integer
                        ? biggerType.IsBiggerThan(game, IntValue)
                        : biggerType.IsBiggerThan(game, DateValue));

                case ComparatorType.IsSmallerThan:
                    return TypeManager is INumberType smallerType && (TypeManager.ValueType == ItemValueType.Integer
                        ? smallerType.IsSmallerThan(game, IntValue)
                        : smallerType.IsSmallerThan(game, DateValue));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}