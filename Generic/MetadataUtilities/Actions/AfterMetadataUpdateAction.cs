using KNARZhelper;
using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;

namespace MetadataUtilities.Actions
{
    public class AfterMetadataUpdateAction : BaseAction
    {
        private static AfterMetadataUpdateAction _instance;
        private bool _haveToExecActions;
        private bool _haveToMerge;
        private bool _haveToRemove;

        private AfterMetadataUpdateAction(Settings settings) : base(settings)
        {
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressMergingItems");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogMergedMetadataMessage";

        public static AfterMetadataUpdateAction Instance(Settings settings) => _instance ?? (_instance = new AfterMetadataUpdateAction(settings));

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
                result |= RemoveUnwantedAction.Instance(Settings).Execute(game, ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (Settings.MergeMetadataOnMetadataUpdate && game.ExecuteMergeRules && _haveToMerge)
            {
                result |= MergeAction.Instance(Settings).Execute(game, ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (game.ExecuteConditionalActions && _haveToExecActions)
            {
                result |= ExecuteConditionalActionsAction.Instance(Settings).Execute(game, ActionModifierType.IsCombi, item, isBulkAction);
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
                RemoveUnwantedAction.Instance(Settings).FollowUp(ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (Settings.MergeMetadataOnMetadataUpdate && _haveToMerge)
            {
                MergeAction.Instance(Settings).FollowUp(ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (_haveToExecActions)
            {
                ExecuteConditionalActionsAction.Instance(Settings).FollowUp(ActionModifierType.IsCombi, item, isBulkAction);
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
                _haveToRemove = RemoveUnwantedAction.Instance(Settings).Prepare(ActionModifierType.IsCombi, item, isBulkAction);
                result |= _haveToRemove;
            }

            if (Settings.MergeMetadataOnMetadataUpdate)
            {
                _haveToMerge = MergeAction.Instance(Settings).Prepare(ActionModifierType.IsCombi, item, isBulkAction);
                result |= _haveToMerge;
            }

            _haveToExecActions = ExecuteConditionalActionsAction.Instance(Settings).Prepare(ActionModifierType.IsCombi, item, isBulkAction);
            result |= _haveToExecActions;

            return result;
        }
    }
}