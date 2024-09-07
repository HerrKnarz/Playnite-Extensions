﻿using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using KNARZhelper.Enum;

namespace MetadataUtilities.Models
{
    public enum ActionType
    {
        AddObject,
        RemoveObject,
        ClearField
    }

    public static class ActionHelper
    {
        public static ActionType ToActionType(this string str) =>
            int.TryParse(str, out int intValue) && intValue >= 0 && intValue <= 2
                ? (ActionType)intValue
                : throw new ArgumentOutOfRangeException(nameof(str), str, null);
    }

    public class Action : MetadataObject
    {
        private ActionType _actionType = ActionType.AddObject;
        private DateTime _dateValue;
        private int _intValue;

        public Action(Settings settings) : base(settings)
        {
        }

        public ActionType ActionType
        {
            get => _actionType;
            set
            {
                SetValue(ref _actionType, value);

                if (value == ActionType.ClearField)
                {
                    Name = string.Empty;
                }
            }
        }

        public DateTime DateValue
        {
            get => _dateValue;
            set => SetValue(ref _dateValue, value);
        }

        public int IntValue
        {
            get => _intValue;
            set => SetValue(ref _intValue, value);
        }

        [DontSerialize]
        public new string ToString
        {
            get
            {
                switch (TypeManager.ValueType)
                {
                    case ItemValueType.Integer:
                        return $"{ActionType.GetEnumDisplayName()} {TypeLabel} {IntValue}";

                    case ItemValueType.Date:
                        return $"{ActionType.GetEnumDisplayName()} {TypeLabel} {DateValue:yyyy-MM-dd}";

                    case ItemValueType.ItemList:
                    case ItemValueType.Media:
                    case ItemValueType.None:
                    case ItemValueType.String:
                    default:
                        return $"{ActionType.GetEnumDisplayName()} {TypeAndName}";
                }
            }
        }

        public bool Execute(Game game)
        {
            switch (ActionType)
            {
                case ActionType.AddObject:
                    switch (TypeManager.ValueType)
                    {
                        case ItemValueType.Integer:
                            return TypeManager.AddValueToGame(game, IntValue);

                        case ItemValueType.Date:
                            return TypeManager.AddValueToGame(game, DateValue);

                        case ItemValueType.ItemList:
                        case ItemValueType.Media:
                        case ItemValueType.None:
                        case ItemValueType.String:
                        default:
                            return AddToGame(game);
                    }
                case ActionType.RemoveObject:
                    return RemoveFromGame(game);

                case ActionType.ClearField:
                    TypeManager.EmptyFieldInGame(game);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}