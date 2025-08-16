﻿using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeTimePlayed : BaseIntegerType
    {
        public override bool CanBeClearedInGame => false;
        public override bool CanBeEmptyInGame => false;
        public override bool CanBeSetByMetadataAddOn => false;
        public override bool IsDefaultToCopy => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCTimePlayed");
        public override FieldType Type => FieldType.TimePlayed;

        public override bool AddValueToGame(Game game, int? value) =>
            API.Instance.MainView.UIDispatcher.Invoke(() =>
            {
                game.Playtime = (ulong)(value ?? 0);

                return true;
            });

        public override void EmptyFieldInGame(Game game)
        {
        }

        public override bool FieldInGameIsEmpty(Game game) => false;

        public override ulong? GetValue(Game game) => game.Playtime;
    }
}