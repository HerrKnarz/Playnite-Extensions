﻿using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    internal class TypeDescription : BaseStringType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameDescriptionTitle");
        public override FieldType Type => FieldType.Description;

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.Description = default);

        public override bool FieldInGameIsEmpty(Game game) => !game?.Description?.Trim().Any() ?? true;

        public override bool GameContainsValue(Game game, string value) => value != null && (game?.Description?.RegExIsMatch(value) ?? false);
    }
}