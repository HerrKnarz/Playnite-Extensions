using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;

namespace KNARZhelper.DatabaseObjectTypes
{
    internal class TypeReleaseDate : BaseEditableDateType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameReleaseDateTitle");
        public override FieldType Type => FieldType.ReleaseDate;

        public override bool AddValueToGame(Game game, DateTime? value) =>
            API.Instance.MainView.UIDispatcher.Invoke(() =>
            {
                game.ReleaseDate = new ReleaseDate(value ?? default);

                return true;
            });

        public override void EmptyFieldInGame(Game game) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => game.ReleaseDate = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.ReleaseDate.HasValue;

        public override DateTime? GetValue(Game game) => game.ReleaseDate?.Date;
    }
}