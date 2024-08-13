using System;
using KNARZhelper;
using Playnite.SDK.Models;

namespace MetadataUtilities.Models
{
    public enum ComparatorType
    {
        Contains,
        DoesNotContain,
        IsEmpty
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