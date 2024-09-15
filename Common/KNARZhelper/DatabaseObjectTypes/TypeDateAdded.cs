using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeDateAdded : BaseDateType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCDateAddedLabel");
        public override FieldType Type => FieldType.DateAdded;

        public override bool AddValueToGame<T>(Game game, T value) => false;

        public override bool FieldInGameIsEmpty(Game game) => !game.Added.HasValue;

        public override DateTime? GetValue(Game game) => game.Added;
    }
}