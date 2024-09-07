using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using KNARZhelper.Enum;

namespace MetadataUtilities.Models
{
    public enum ComparatorType
    {
        Contains,
        DoesNotContain,
        IsEmpty,
        IsNotEmpty,
        IsBiggerThan,
        IsSmallerThan
    }

    public static class ConditionHelper
    {
        public static ComparatorType ToComparatorType(this string str) =>
            int.TryParse(str, out int intValue) && intValue >= 0 && intValue <= 2
                ? (ComparatorType)intValue
                : throw new ArgumentOutOfRangeException(nameof(str), str, null);
    }

    public class Condition : MetadataObject
    {
        private ComparatorType _comparator = ComparatorType.Contains;

        private DateTime? _dateValue;
        private int? _intValue;

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

                    case ItemValueType.ItemList:
                    case ItemValueType.Media:
                    case ItemValueType.None:
                    case ItemValueType.String:
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
                    return ExistsInGame(game);

                case ComparatorType.DoesNotContain:
                    return !ExistsInGame(game);

                case ComparatorType.IsEmpty:
                    return TypeManager.FieldInGameIsEmpty(game);

                case ComparatorType.IsNotEmpty:
                    return !TypeManager.FieldInGameIsEmpty(game);

                case ComparatorType.IsBiggerThan:
                    return TypeManager.ValueType == ItemValueType.Integer ? TypeManager.IsBiggerThan(game, IntValue) : TypeManager.IsBiggerThan(game, DateValue);

                case ComparatorType.IsSmallerThan:
                    return TypeManager.ValueType == ItemValueType.Integer ? TypeManager.IsSmallerThan(game, IntValue) : TypeManager.IsSmallerThan(game, DateValue);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}