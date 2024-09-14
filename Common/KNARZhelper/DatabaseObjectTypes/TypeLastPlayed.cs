using Playnite.SDK;
using System;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeLastPlayed : BaseDateType
    {
        public override bool CanBeEmptyInGame => true;
        public override string LabelSingular => ResourceProvider.GetString("LOCLastPlayedLabel");
        public override FieldType Type => FieldType.LastPlayed;

        public override bool FieldInGameIsEmpty(Game game) => !game.LastActivity.HasValue;

        public override bool IsBiggerThan(Game game, DateTime? value) => value != null && game.LastActivity > value;

        public override bool IsSmallerThan(Game game, DateTime? value) => value != null && game.LastActivity < value;
    }
}