﻿using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    internal class TypeNotes : BaseStringType
    {
        public override bool CanBeSetByMetadataAddOn => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCNotesLabel");
        public override FieldType Type => FieldType.Notes;

        public override bool AddValueToGame(Game game, string value)
        {
            API.Instance.MainView.UIDispatcher.Invoke(() =>
            {
                game.Notes = value;
            });

            return true;
        }

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.Notes = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.Notes.Trim().Any();

        public override bool GameContainsValue(Game game, string value) => game.Notes.RegExIsMatch(value);

        public override string GetValue(Game game) => game.Notes;
    }
}