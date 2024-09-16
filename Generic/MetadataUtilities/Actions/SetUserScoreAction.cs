using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;

namespace MetadataUtilities.Actions
{
    public class SetUserScoreAction : BaseAction
    {
        private static readonly object _mutex = new object();
        private static SetUserScoreAction _instance;

        private SetUserScoreAction(MetadataUtilities plugin) => Settings = plugin.Settings.Settings;

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressSettingUserScore");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogSetUserScoreMessage";

        public Settings Settings { get; set; }

        public static SetUserScoreAction Instance(MetadataUtilities plugin)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new SetUserScoreAction(plugin);
                }
            }

            return _instance;
        }

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            if (item == null)
                return false;

            game.Game.UserScore = (int)item;
            API.Instance.Database.Games.Update(game.Game);

            return true;
        }
    }
}