using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using MetadataUtilities.Enums;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;

namespace MetadataUtilities.Models
{
    public class Action : MetadataObject
    {
        private ActionType _actionType = ActionType.AddObject;
        private DateTime _dateValue;
        private int _intValue;

        public Action(FieldType type, string name = default) : base(type, name)
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
                    case ItemValueType.Boolean:
                        return $"{ActionType.GetEnumDisplayName()} {TypeLabel}";

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
                            return TypeManager is IValueType intType && intType.AddValueToGame(game, IntValue);

                        case ItemValueType.Date:
                            return TypeManager is IValueType dateType && dateType.AddValueToGame(game, DateValue);

                        case ItemValueType.Boolean:
                            return TypeManager is IValueType boolType && boolType.AddValueToGame(game, true);

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
                    if (TypeManager is IClearAbleType type)
                    {
                        type.EmptyFieldInGame(game);
                    }

                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}