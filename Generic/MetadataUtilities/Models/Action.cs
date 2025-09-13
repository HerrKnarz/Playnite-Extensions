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
        private ulong _ulongValue;
        private string _stringValue;

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

        public ulong UlongValue
        {
            get => _ulongValue;
            set => SetValue(ref _ulongValue, value);
        }

        public string StringValue
        {
            get => _stringValue;
            set => SetValue(ref _stringValue, value);
        }

        [DontSerialize]
        public new string ToString
        {
            get
            {
                if (ActionType == ActionType.ClearField)
                {
                    return $"{ActionType.GetEnumDisplayName()} {TypeLabel}";
                }

                switch (TypeManager.ValueType)
                {
                    case ItemValueType.Boolean:
                        return $"{ActionType.GetEnumDisplayName()} {TypeLabel}";

                    case ItemValueType.Integer:
                        return $"{ActionType.GetEnumDisplayName()} {TypeLabel} {IntValue}";

                    case ItemValueType.Date:
                        return $"{ActionType.GetEnumDisplayName()} {TypeLabel} {DateValue:yyyy-MM-dd}";

                    case ItemValueType.Media:
                    case ItemValueType.String:
                        return $"{ActionType.GetEnumDisplayName()} {TypeLabel} \"{StringValue}\"";

                    case ItemValueType.Ulong:
                        return $"{ActionType.GetEnumDisplayName()} {TypeLabel} {UlongValue}";

                    case ItemValueType.ItemList:
                    case ItemValueType.None:
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

                        case ItemValueType.Media:
                        case ItemValueType.String:
                            return TypeManager is IValueType stringType && stringType.AddValueToGame(game, StringValue);

                        case ItemValueType.Ulong:
                            return TypeManager is IValueType ulongType && ulongType.AddValueToGame(game, UlongValue);

                        case ItemValueType.ItemList:
                        case ItemValueType.None:
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