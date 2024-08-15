using System;
using KNARZhelper;
using Playnite.SDK.Data;
using Playnite.SDK.Models;

namespace MetadataUtilities.Models
{
    public enum ComparatorType
    {
        Contains,
        DoesNotContain,
        IsEmpty
    }

    public static class ConditionHelper
    {
        public static ComparatorType ToComparatorType(this string str)
        {
            if (int.TryParse(str, out int intValue) && intValue >= 0 && intValue <= 2)
            {
                return (ComparatorType)intValue;
            }

            throw new ArgumentOutOfRangeException(nameof(str), str, null);
        }
    }

    public class Condition : MetadataObject
    {
        private ComparatorType _comparator = ComparatorType.Contains;

        public Condition(Settings settings) : base(settings)
        {
        }

        public ComparatorType Comparator
        {
            get => _comparator;
            set => SetValue(ref _comparator, value);
        }

        [DontSerialize]
        public new string ToString => $"{TypeAsString} {Comparator.GetEnumDisplayName()} {Name}";

        public bool IsTrue(Game game)
        {
            switch (Comparator)
            {
                case ComparatorType.Contains:
                    return DatabaseObjectHelper.DbObjectInGame(game, Type, Id);

                case ComparatorType.DoesNotContain:
                    return !DatabaseObjectHelper.DbObjectInGame(game, Type, Id);

                case ComparatorType.IsEmpty:
                    return DatabaseObjectHelper.FieldInGameIsEmpty(game, Type);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}