﻿using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    internal class TypeManual : BaseStringType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameManualTitle");
        public override FieldType Type => FieldType.Manual;

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.Manual = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.Manual?.Trim().Any() ?? true;

        public override bool GameContainsValue(Game game, string value) => game.Manual?.RegExIsMatch(value) ?? false;
    }
}