﻿using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    public class TypeCommunityScore : BaseIntegerType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCCommunityScore");
        public override FieldType Type => FieldType.CommunityScore;

        public override bool AddValueToGame(Game game, int? value) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => (game.CommunityScore = value) != null);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.CommunityScore = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.CommunityScore.HasValue;

        public override int? GetValue(Game game) => game.CommunityScore;
    }
}