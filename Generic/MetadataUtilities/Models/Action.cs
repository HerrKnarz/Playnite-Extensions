using System;
using System.Collections.Generic;
using System.Linq;
using KNARZhelper;
using Playnite.SDK.Models;

namespace MetadataUtilities.Models
{
    public enum ActionType
    {
        AddObject,
        RemoveObject,
        ClearField
    }

    public class Action : SettableMetadataObject
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

        public bool Execute(Game game)
        {
            switch (ActionType)
            {
                case ActionType.AddObject:
                    return DatabaseObjectHelper.AddDbObjectToGame(game, Type, Name);

                case ActionType.RemoveObject:
                    return DatabaseObjectHelper.RemoveObjectFromGame(game, Type, Id);

                case ActionType.ClearField:
                    DatabaseObjectHelper.EmptyFieldInGame(game, Type);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}