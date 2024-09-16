using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;

namespace MetadataUtilities.Actions
{
    public class AfterMetadataUpdateAction : BaseAction
    {
        private static readonly object _mutex = new object();
        private static AfterMetadataUpdateAction _instance;

        private AfterMetadataUpdateAction(Settings settings) : base(settings)
        {
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressMergingItems");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogMergedMetadataMessage";

        public static AfterMetadataUpdateAction Instance(Settings settings)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new AfterMetadataUpdateAction(settings);
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

            bool result = false;

            if (Settings.RemoveUnwantedOnMetadataUpdate && game.ExecuteRemoveUnwanted)
            {
                result |= RemoveUnwantedAction.Instance(Settings).Execute(game, ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (Settings.MergeMetadataOnMetadataUpdate && game.ExecuteMergeRules)
            {
                result |= MergeAction.Instance(Settings).Execute(game, ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (game.ExecuteConditionalActions)
            {
                result |= ExecuteConditionalActionsAction.Instance(Settings).Execute(game, ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (result)
            {
                API.Instance.Database.Games.Update(game.Game);
            }

            return result;
        }

        public override void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (Settings.RemoveUnwantedOnMetadataUpdate)
            {
                RemoveUnwantedAction.Instance(Settings).FollowUp(ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (Settings.MergeMetadataOnMetadataUpdate)
            {
                MergeAction.Instance(Settings).FollowUp(ActionModifierType.IsCombi, item, isBulkAction);
            }

            ExecuteConditionalActionsAction.Instance(Settings).FollowUp(ActionModifierType.IsCombi, item, isBulkAction);
        }

        public override bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            bool result = true;

            if (Settings.RemoveUnwantedOnMetadataUpdate)
            {
                result &= RemoveUnwantedAction.Instance(Settings).Prepare(ActionModifierType.IsCombi, item, isBulkAction);
            }

            if (Settings.MergeMetadataOnMetadataUpdate)
            {
                result &= MergeAction.Instance(Settings).Prepare(ActionModifierType.IsCombi, item, isBulkAction);
            }

            result &= ExecuteConditionalActionsAction.Instance(Settings).Prepare(ActionModifierType.IsCombi, item, isBulkAction);

            return result;
        }
    }
}