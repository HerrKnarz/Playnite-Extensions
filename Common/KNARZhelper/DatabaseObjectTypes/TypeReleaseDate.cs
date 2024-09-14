using Playnite.SDK;
using System;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace KNARZhelper.DatabaseObjectTypes
{
    internal class TypeReleaseDate : BaseEditableDateType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameReleaseDateTitle");
        public override FieldType Type => FieldType.ReleaseDate;

        public override bool AddValueToGame(Game game, DateTime? value) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => (game.ReleaseDate = new ReleaseDate(value ?? default)) != null);

        public override void EmptyFieldInGame(Game game) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => game.ReleaseDate = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.ReleaseDate.HasValue;

        public override bool IsBiggerThan(Game game, DateTime? value) => game.ReleaseDate?.Date > value;

        public override bool IsSmallerThan(Game game, DateTime? value) => game.ReleaseDate?.Date < value;
    }
}