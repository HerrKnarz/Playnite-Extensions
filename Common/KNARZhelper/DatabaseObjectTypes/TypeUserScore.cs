﻿using Playnite.SDK;
using System;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeUserScore : BaseType
    {
        public override bool CanBeAdded => false;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => true;
        public override bool CanBeModified => false;
        public override bool CanBeSetInGame => true;
        public override bool IsList => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCUserScore");
        public override FieldType Type => FieldType.UserScore;
        public override ItemValueType ValueType => ItemValueType.Integer;

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.UserScore = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.UserScore.HasValue;

        public override bool IsBiggerThan<T>(Game game, T value) =>
            (value != null || value is int) && game.UserScore > (value as int?);

        public override bool IsSmallerThan<T>(Game game, T value) =>
            (value != null || value is int) && game.UserScore < (value as int?);
    }
}