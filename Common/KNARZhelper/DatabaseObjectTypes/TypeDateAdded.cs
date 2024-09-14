using Playnite.SDK;
using System;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeDateAdded : BaseDateType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCDateAddedLabel");
        public override FieldType Type => FieldType.DateAdded;

        public override bool FieldInGameIsEmpty(Game game) => !game.Added.HasValue;

        public override bool IsBiggerThan(Game game, DateTime? value) => value != null && game.Added > value;

        public override bool IsSmallerThan(Game game, DateTime? value) => value != null && game.Added < value;
    }
}