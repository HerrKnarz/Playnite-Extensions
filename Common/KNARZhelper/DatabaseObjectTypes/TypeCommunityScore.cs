﻿using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace KNARZhelper.DatabaseObjectTypes
{
    internal class TypeCommunityScore : BaseType
    {
        public override bool CanBeAdded => false;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => true;
        public override bool CanBeModified => false;
        public override bool CanBeSetInGame => true;
        public override bool IsList => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCCommunityScore");
        public override FieldType Type => FieldType.CommunityScore;
        public override ItemValueType ValueType => ItemValueType.Integer;

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.CommunityScore = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.CommunityScore.HasValue;

        public override bool IsBiggerThan<T>(Game game, T value) =>
            (value != null || value is int) && game.CommunityScore > (value as int?);

        public override bool IsSmallerThan<T>(Game game, T value) =>
            (value != null || value is int) && game.CommunityScore < (value as int?);
    }
}