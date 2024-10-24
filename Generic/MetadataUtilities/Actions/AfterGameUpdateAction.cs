using KNARZhelper;
using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;

namespace MetadataUtilities.Actions
{
    public class AfterGameUpdateAction : BaseAction
    {
        private static AfterGameUpdateAction _instance;
        private bool _haveToExecActions;
        private bool _haveToMerge;
        private bool _haveToRemove;

        private AfterGameUpdateAction()
        {
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressMergingItems");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogMergedMetadataMessage";

        public static AfterGameUpdateAction Instance() => _instance ?? (_instance = new AfterGameUpdateAction());

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (Settings.WriteDebugLog)
            {
                Log.Debug($"===> Game \"{game.Game.Name}\"  =======================================");
                Log.Debug($"ExecuteRemoveUnwanted: {game.ExecuteRemoveUnwanted}");
                Log.Debug($"ExecuteMergeRules: {game.ExecuteMergeRules}");
                Log.Debug($"ExecuteConditionalActions: {game.ExecuteConditionalActions}");
            }

            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            var result = false;

            if (Settings.RemoveUnwantedOnMetadataUpdate && game.ExecuteRemoveUnwanted && _haveToRemove)
            {
                result |= RemoveUnwantedAction.Instance().Execute(game, ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (Settings.MergeMetadataOnMetadataUpdate && game.ExecuteMergeRules && _haveToMerge)
            {
                result |= MergeAction.Instance().Execute(game, ActionModifierType.IsCombi, item, isBulkAction);
            }

            var gameIsKnown = ControlCenter.Instance.KnownGames.Contains(game.Game.Id);
            var beforeMetadataDownload = !gameIsKnown && !ControlCenter.Instance.NewGames.Contains(game.Game.Id);

            if (beforeMetadataDownload)
            {
                ControlCenter.Instance.NewGames.Add(game.Game.Id);
            }

            if (game.ExecuteConditionalActions && _haveToExecActions)
            {
                result |= ExecuteConditionalActionsAction.Instance().Execute(game, ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (result)
            {
                _gamesAffected.Add(game.Game);
            }

            return result;
        }

        public override void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (Settings.RemoveUnwantedOnMetadataUpdate && _haveToRemove)
            {
                RemoveUnwantedAction.Instance().FollowUp(ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (Settings.MergeMetadataOnMetadataUpdate && _haveToMerge)
            {
                MergeAction.Instance().FollowUp(ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (_haveToExecActions)
            {
                ExecuteConditionalActionsAction.Instance().FollowUp(ActionModifierType.IsCombi, item, isBulkAction);
            }

            base.FollowUp(ActionModifierType.None, item, isBulkAction);
        }

        public override bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (Settings.WriteDebugLog)
            {
                Log.Debug($"===> preparing AfterMetadataUpdate =======================");
                Log.Debug($"RemoveUnwantedOnMetadataUpdate: {Settings.RemoveUnwantedOnMetadataUpdate}");
                Log.Debug($"MergeMetadataOnMetadataUpdate: {Settings.MergeMetadataOnMetadataUpdate}");
            }

            if (!base.Prepare(ActionModifierType.IsCombi, item, isBulkAction))
            {
                return false;
            }

            var result = false;

            if (Settings.RemoveUnwantedOnMetadataUpdate)
            {
                _haveToRemove = RemoveUnwantedAction.Instance().Prepare(ActionModifierType.IsCombi, item, isBulkAction);
                result |= _haveToRemove;
            }

            if (Settings.MergeMetadataOnMetadataUpdate)
            {
                _haveToMerge = MergeAction.Instance().Prepare(ActionModifierType.IsCombi, item, isBulkAction);
                result |= _haveToMerge;
            }

            _haveToExecActions = ExecuteConditionalActionsAction.Instance().Prepare(ActionModifierType.IsCombi, item, isBulkAction);
            result |= _haveToExecActions;

            return result;
        }
    }
}