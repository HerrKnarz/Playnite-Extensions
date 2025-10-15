﻿using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    public class TypePlayCount : BaseUlongType
    {
        public override bool CanBeClearedInGame => false;
        public override bool CanBeEmptyInGame => false;
        public override bool CanBeSetByMetadataAddOn => false;
        public override bool IsDefaultToCopy => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCPlayCountLabel");
        public override FieldType Type => FieldType.PlayCount;

        public override bool AddValueToGame(Game game, ulong? value) =>
            API.Instance.MainView.UIDispatcher.Invoke(() =>
            {
                game.PlayCount = value ?? 0;

                return true;
            });

        public override void EmptyFieldInGame(Game game)
        {
        }

        public override bool FieldInGameIsEmpty(Game game) => false;

        public override ulong GetValue(Game game) => game.PlayCount;
    }
}