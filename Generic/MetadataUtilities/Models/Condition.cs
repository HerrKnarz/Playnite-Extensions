using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using MetadataUtilities.Enums;
using Playnite.SDK;
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
        private ulong? _ulongValue;
        private string _stringValue;

        public Condition(FieldType type, string name = default) : base(type, name)
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

        public ulong? UlongValue
        {
            get => _ulongValue;
            set => SetValue(ref _ulongValue, value);
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
                if (Comparator == ComparatorType.GameIsNew)
                {
                    return ResourceProvider.GetString("LOCMetadataUtilitiesGameIsNew");
                }

                if (Comparator == ComparatorType.IsEmpty || Comparator == ComparatorType.IsNotEmpty)
                {
                    return $"{TypeLabel} {Comparator.GetEnumDisplayName()}";
                }

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

                    case ItemValueType.Ulong:
                        return $"{TypeLabel} {Comparator.GetEnumDisplayName()} {UlongValue}";

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

                        case ItemValueType.Ulong:
                            return containsType.GameContainsValue(game, UlongValue);

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

                        case ItemValueType.Ulong:
                            return !notContainsType.GameContainsValue(game, UlongValue);

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
                    switch (TypeManager.ValueType)
                    {
                        case ItemValueType.Integer:
                            return TypeManager is INumberType biggerIntType && biggerIntType.IsBiggerThan(game, IntValue);
                        case ItemValueType.Date:
                            return TypeManager is INumberType biggerDateType && biggerDateType.IsBiggerThan(game, DateValue);
                        case ItemValueType.Ulong:
                            return TypeManager is INumberType biggerUlongType && biggerUlongType.IsBiggerThan(game, UlongValue);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case ComparatorType.IsSmallerThan:
                    switch (TypeManager.ValueType)
                    {
                        case ItemValueType.Integer:
                            return TypeManager is INumberType smallerIntType && smallerIntType.IsSmallerThan(game, IntValue);
                        case ItemValueType.Date:
                            return TypeManager is INumberType smallerDateType && smallerDateType.IsSmallerThan(game, DateValue);
                        case ItemValueType.Ulong:
                            return TypeManager is INumberType smallerUlongType && smallerUlongType.IsSmallerThan(game, UlongValue);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case ComparatorType.GameIsNew:
                    return ControlCenter.Instance.NewGames.Contains(game.Id);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}