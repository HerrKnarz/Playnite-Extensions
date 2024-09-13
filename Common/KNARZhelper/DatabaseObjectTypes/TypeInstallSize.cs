﻿using Playnite.SDK;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeInstallSize : BaseIntegerType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCInstallSizeLabel");
        public override FieldType Type => FieldType.InstallSize;

        public override bool AddValueToGame(Game game, int? value) =>
            API.Instance.MainView.UIDispatcher.Invoke(() =>
            {
                game.InstallSize = (ulong)(value ?? 0);

                return true;
            });

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.InstallSize = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.InstallSize.HasValue;

        public override bool IsBiggerThan(Game game, int value) => game.InstallSize > (ulong)value;

        public override bool IsSmallerThan(Game game, int value) => game.InstallSize < (ulong)value;
    }
}