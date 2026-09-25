using KNARZhelper;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.Enum;
using MetadataUtilities.Enums;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;

namespace MetadataUtilities.Models
{
    public class Condition : MetadataObject
    {
        private ComparatorType _comparator = ComparatorType.Contains;

        private ConditionPropertyType _conditionPropertyType = ConditionPropertyType.Value;

        private DateTime? _dateValue;
        private int? _intValue;
        private string _stringValue;
        private ulong? _ulongValue;

        public Condition(FieldType type, string name = default) : base(type, name)
        {
        }

        public ComparatorType Comparator
        {
            get => _comparator;
            set => SetValue(ref _comparator, value);
        }

        public ConditionPropertyType ConditionPropertyType
        {
            get => _conditionPropertyType;
            set => SetValue(ref _conditionPropertyType, value);
        }

        public DateTime? DateValue
        {
            get => _dateValue;
            set => SetValue(ref _dateValue, value);
        }

        public int? IntValue
        {
            get => _intValue;
            set => SetValue(ref _intValue, value);
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
                if (Comparator == ComparatorType.GameIsNew)
                {
                    return ResourceProvider.GetString("LOCMetadataUtilitiesGameIsNew");
                }

                if (Comparator == ComparatorType.IsEmpty || Comparator == ComparatorType.IsNotEmpty)
                {
                    return GetDisplayString();
                }

                switch (TypeManager.ValueType)
                {
                    case ItemValueType.Boolean:
                        return GetDisplayString();

                    case ItemValueType.Integer:
                        return $"{GetDisplayString()} {IntValue}";

                    case ItemValueType.Date:
                        return $"{GetDisplayString()} {DateValue?.ToString("yyyy-MM-dd")}";

                    case ItemValueType.String:
                        return $"{GetDisplayString()} {StringValue}";

                    case ItemValueType.Ulong:
                        return $"{GetDisplayString()} {UlongValue}";

                    case ItemValueType.Media:
                        return Comparator.IsOneOf(ComparatorType.IsBiggerThan, ComparatorType.IsSmallerThan, ComparatorType.Equals)
                            ? $"{GetDisplayString()} {IntValue} {(ConditionPropertyType == ConditionPropertyType.FileSize ? "KB" : "px")}"
                            : $"{GetDisplayString()} {Name}";

                    case ItemValueType.ItemList:
                    case ItemValueType.None:
                    default:
                        return $"{GetDisplayString()} {Name}";
                }
            }
        }

        public ulong? UlongValue
        {
            get => _ulongValue;
            set => SetValue(ref _ulongValue, value);
        }

        public bool IsTrue(Game game)
        {
            var result = false;

            var logMessagePrefix = $"Condition {ToString.PadRight(50, '=').Substring(0, 50)} for game {game.Name.PadRight(50, '=').Substring(0, 50)}==> ";

            Log.Debug(ControlCenter.Instance.Settings.WriteDebugLog, logMessagePrefix + "checking...");
            try
            {
                if (Comparator == ComparatorType.GameIsNew)
                {
                    return ControlCenter.Instance.NewGames.Contains(game.Id);
                }

                switch (TypeManager.ValueType)
                {
                    case ItemValueType.Boolean:
                        result = IsTrueBool(game);
                        break;

                    case ItemValueType.Integer:
                        result = IsTrueInt(game);
                        break;

                    case ItemValueType.Date:
                        result = IsTrueDate(game);
                        break;

                    case ItemValueType.String:
                        result = IsTrueString(game);
                        break;

                    case ItemValueType.Ulong:
                        result = IsTrueUlong(game);
                        break;

                    case ItemValueType.Media:
                        result = IsTrueMedia(game);
                        break;

                    case ItemValueType.ItemList:
                        result = IsTrueItemList(game);
                        break;

                    case ItemValueType.LinkList:
                        result = IsTrueLinkList(game);
                        break;

                    case ItemValueType.None:
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, logMessagePrefix + "error!");
                return false;
            }
            finally
            {
                Log.Debug(ControlCenter.Instance.Settings.WriteDebugLog, logMessagePrefix + $"result => {result.ToString().ToUpper()}");
            }

            return result;
        }

        private string GetDisplayString() => (ConditionPropertyType != ConditionPropertyType.Value
            ? $"{TypeLabel} {ConditionPropertyType.GetEnumDisplayNameWithType()}"
            : TypeLabelInGame) + " " + Comparator.GetEnumDisplayName();

        private bool IsTrueBool(Game game)
        {
            if (!(TypeManager is IValueType boolType))
            {
                return false;
            }

            switch (Comparator)
            {
                case ComparatorType.Contains:
                case ComparatorType.Equals:
                case ComparatorType.IsNotEmpty:
                    return boolType.GameContainsValue(game, true);

                case ComparatorType.DoesNotContain:
                case ComparatorType.DoesntEqual:
                case ComparatorType.IsEmpty:
                    return !boolType.GameContainsValue(game, true);

                default:
                    return false;
            }
        }

        private bool IsTrueDate(Game game)
        {
            if (!(TypeManager is INumberType intType))
            {
                return false;
            }

            switch (Comparator)
            {
                case ComparatorType.IsBiggerThan:
                    return intType.IsBiggerThan(game, DateValue);

                case ComparatorType.IsSmallerThan:
                    return intType.IsSmallerThan(game, DateValue);

                case ComparatorType.Contains:
                case ComparatorType.Equals:
                    return TypeManager is IValueType equalType && equalType.GameContainsValue(game, DateValue);

                case ComparatorType.DoesNotContain:
                case ComparatorType.DoesntEqual:
                    return !(TypeManager is IValueType notEqualType && notEqualType.GameContainsValue(game, DateValue));

                case ComparatorType.IsEmpty:
                    return TypeManager is IClearAbleType emptyType && emptyType.FieldInGameIsEmpty(game);

                case ComparatorType.IsNotEmpty:
                    return TypeManager is IClearAbleType notEmptyType && !notEmptyType.FieldInGameIsEmpty(game);

                default:
                    return false;
            }
        }

        private bool IsTrueInt(Game game)
        {
            if (!(TypeManager is INumberType intType))
            {
                return false;
            }

            switch (Comparator)
            {
                case ComparatorType.IsBiggerThan:
                    return intType.IsBiggerThan(game, IntValue);

                case ComparatorType.IsSmallerThan:
                    return intType.IsSmallerThan(game, IntValue);

                case ComparatorType.Contains:
                case ComparatorType.Equals:
                    return TypeManager is IValueType equalType && equalType.GameContainsValue(game, IntValue);

                case ComparatorType.DoesNotContain:
                case ComparatorType.DoesntEqual:
                    return !(TypeManager is IValueType notEqualType && notEqualType.GameContainsValue(game, IntValue));

                case ComparatorType.IsEmpty:
                    return TypeManager is IClearAbleType emptyType && emptyType.FieldInGameIsEmpty(game);

                case ComparatorType.IsNotEmpty:
                    return TypeManager is IClearAbleType notEmptyType && !notEmptyType.FieldInGameIsEmpty(game);

                default:
                    return false;
            }
        }

        private bool IsTrueItemList(Game game)
        {
            if (!(TypeManager is IValueType valueType))
            {
                return false;
            }

            switch (Comparator)
            {
                case ComparatorType.Contains:
                case ComparatorType.Equals:
                    return ExistsInGame(game);

                case ComparatorType.DoesNotContain:
                case ComparatorType.DoesntEqual:
                    return !ExistsInGame(game);

                case ComparatorType.IsEmpty:
                    return TypeManager is IClearAbleType emptyType && emptyType.FieldInGameIsEmpty(game);

                case ComparatorType.IsNotEmpty:
                    return TypeManager is IClearAbleType notEmptyType && !notEmptyType.FieldInGameIsEmpty(game);

                default:
                    return false;
            }
        }

        private bool IsTrueLinkList(Game game)
        {
            switch (Comparator)
            {
                case ComparatorType.IsEmpty:
                    return TypeManager is IClearAbleType emptyType && emptyType.FieldInGameIsEmpty(game);

                case ComparatorType.IsNotEmpty:
                    return TypeManager is IClearAbleType notEmptyType && !notEmptyType.FieldInGameIsEmpty(game);

                default:
                    return false;
            }
        }

        private bool IsTrueMedia(Game game)
        {
            if (!(TypeManager is IImageType imageType))
            {
                return false;
            }

            var isIntValue = ConditionPropertyType.IsOneOf(ConditionPropertyType.FileSize, ConditionPropertyType.Width, ConditionPropertyType.Height);

            var valueToCompare = 0;

            if (isIntValue)
            {
                if (TypeManager is IClearAbleType emptyType && emptyType.FieldInGameIsEmpty(game))
                {
                    return false;
                }

                switch (ConditionPropertyType)
                {
                    case ConditionPropertyType.FileSize:
                        valueToCompare = imageType.GetFileSizeInBytes(game) / 1024;
                        break;

                    case ConditionPropertyType.Width:
                        valueToCompare = imageType.GetImageSize(game).Width;
                        break;

                    case ConditionPropertyType.Height:
                        valueToCompare = imageType.GetImageSize(game).Height;
                        break;

                    default:
                        return false;
                }
            }

            switch (Comparator)
            {
                case ComparatorType.IsBiggerThan:
                    return isIntValue && valueToCompare > IntValue;

                case ComparatorType.IsSmallerThan:
                    return isIntValue && valueToCompare < IntValue;

                case ComparatorType.Contains:
                case ComparatorType.Equals:
                    return isIntValue && valueToCompare == IntValue;

                case ComparatorType.DoesNotContain:
                case ComparatorType.DoesntEqual:
                    return isIntValue && valueToCompare != IntValue;

                case ComparatorType.IsEmpty:
                    return TypeManager is IClearAbleType emptyType && emptyType.FieldInGameIsEmpty(game);

                case ComparatorType.IsNotEmpty:
                    return TypeManager is IClearAbleType notEmptyType && !notEmptyType.FieldInGameIsEmpty(game);

                default:
                    return false;
            }
        }

        private bool IsTrueString(Game game)
        {
            if (!(TypeManager is IValueType stringType))
            {
                return false;
            }

            //TODO: maybe allow is Smaller and Bigger for string fields
            switch (Comparator)
            {
                case ComparatorType.Contains:
                case ComparatorType.Equals:
                    return stringType.GameContainsValue(game, StringValue);

                case ComparatorType.DoesNotContain:
                case ComparatorType.DoesntEqual:
                    return !stringType.GameContainsValue(game, StringValue);

                case ComparatorType.IsEmpty:
                    return TypeManager is IClearAbleType emptyType && emptyType.FieldInGameIsEmpty(game);

                case ComparatorType.IsNotEmpty:
                    return TypeManager is IClearAbleType notEmptyType && !notEmptyType.FieldInGameIsEmpty(game);

                default:
                    return false;
            }
        }

        private bool IsTrueUlong(Game game)
        {
            if (!(TypeManager is INumberType ulongType))
            {
                return false;
            }

            switch (Comparator)
            {
                case ComparatorType.IsBiggerThan:
                    return ulongType.IsBiggerThan(game, UlongValue);

                case ComparatorType.IsSmallerThan:
                    return ulongType.IsSmallerThan(game, UlongValue);

                case ComparatorType.Contains:
                case ComparatorType.Equals:
                    return TypeManager is IValueType equalType && equalType.GameContainsValue(game, UlongValue);

                case ComparatorType.DoesNotContain:
                case ComparatorType.DoesntEqual:
                    return !(TypeManager is IValueType notEqualType && notEqualType.GameContainsValue(game, UlongValue));

                case ComparatorType.IsEmpty:
                    return TypeManager is IClearAbleType emptyType && emptyType.FieldInGameIsEmpty(game);

                case ComparatorType.IsNotEmpty:
                    return TypeManager is IClearAbleType notEmptyType && !notEmptyType.FieldInGameIsEmpty(game);

                default:
                    return false;
            }
        }
    }
}
