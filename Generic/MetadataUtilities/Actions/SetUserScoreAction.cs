﻿using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;

namespace MetadataUtilities.Actions
{
    public class SetUserScoreAction : BaseAction
    {
        private static SetUserScoreAction _instance;

        private SetUserScoreAction()
        {
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressSettingUserScore");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogSetUserScoreMessage";

        public static SetUserScoreAction Instance() => _instance ?? (_instance = new SetUserScoreAction());

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            if (item == null)
                return false;

            game.Game.UserScore = (int)item;

            _gamesAffected.Add(game.Game);

            return true;
        }
    }
}