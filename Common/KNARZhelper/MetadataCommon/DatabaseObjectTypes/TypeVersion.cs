﻿using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    internal class TypeVersion : BaseStringType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCVersionLabel");
        public override FieldType Type => FieldType.Version;

        public override bool AddValueToGame(Game game, string value)
        {
            API.Instance.MainView.UIDispatcher.Invoke(() =>
            {
                game.Version = value;
            });

            return true;
        }

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.Version = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.Version?.Trim().Any() == true;

        public override bool GameContainsValue(Game game, string value) => game.Version?.RegExIsMatch(value) ?? false;

        public override string GetValue(Game game) => game.Version ?? string.Empty;
    }
}