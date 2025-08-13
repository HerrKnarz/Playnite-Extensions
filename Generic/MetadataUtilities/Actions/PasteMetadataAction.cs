using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;

namespace MetadataUtilities.Actions
{
    internal class PasteMetadataAction : BaseAction
    {
        private static PasteMetadataAction _instance;

        private PasteMetadataAction()
        {
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressCopyingData");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogCopiedMetadataMessage";

        public static PasteMetadataAction Instance() => _instance ?? (_instance = new PasteMetadataAction());

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None,
            object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            var result = ControlCenter.Instance.GameToCopy.CopyToGame(game.Game);

            if (result)
            {
                _gamesAffected.Add(game.Game);
            }

            return result;
        }

        public override void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            base.FollowUp(actionModifier, item, isBulkAction);

            ControlCenter.Instance.GameToCopy = null;
        }
    }
}
