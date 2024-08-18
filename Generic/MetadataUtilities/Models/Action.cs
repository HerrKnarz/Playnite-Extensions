using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;

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

        [DontSerialize]
        public new string ToString => $"{ActionType.GetEnumDisplayName()} {TypeAndName}";

        public bool Execute(Game game)
        {
            switch (ActionType)
            {
                case ActionType.AddObject:
                    return AddToGame(game);

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