using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;
using System.Linq;

namespace MetadataUtilities.Actions
{
    public class ExecuteConditionalActionsAction : BaseAction
    {
        private static ExecuteConditionalActionsAction _instance;

        private ExecuteConditionalActionsAction(Settings settings) : base(settings)
        {
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressExecutingConditionalActions");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogExecutedConditionalActions";

        public static ExecuteConditionalActionsAction Instance(Settings settings) => _instance ?? (_instance = new ExecuteConditionalActionsAction(settings));

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            var mustUpdate =
                ((ConditionalAction)item)?.CheckAndExecute(game.Game, actionModifier == ActionModifierType.IsManual) ??
                Settings.ConditionalActions.Where(x => x.Enabled).Aggregate(false,
                    (current, conditionalAction) =>
                        current | conditionalAction.CheckAndExecute(game.Game,
                            actionModifier == ActionModifierType.IsManual));

            if (mustUpdate && actionModifier != ActionModifierType.IsCombi)
            {
                _gamesAffected.Add(game.Game);
            }

            return mustUpdate;
        }
    }
}