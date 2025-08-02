using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Actions
{
    public class ExecuteConditionalActionsAction : BaseAction
    {
        private static ExecuteConditionalActionsAction _instance;

        private readonly List<ConditionalAction> _actions = new List<ConditionalAction>();

        private ExecuteConditionalActionsAction()
        {
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressExecutingConditionalActions");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogExecutedConditionalActions";

        public static ExecuteConditionalActionsAction Instance()
        {
            return _instance ?? (_instance = new ExecuteConditionalActionsAction());
        }

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            List<IObjectType> types = FieldTypeHelper.GetAllTypes<IObjectType>().ToList();

            if (Settings.WriteDebugLog)
            {
                Log.Debug($"==== Started executing Conditional Actions on Game \"{game.Game.Name}\" ======================================");

                foreach (IObjectType type in types)
                {
                    Log.Debug($"{type.LabelPlural} before: {string.Join(", ", type.LoadGameMetadata(game.Game).Select(x => x.Name))}");
                }
            }

            bool mustUpdate = _actions
                .Where(x =>
                    actionModifier == ActionModifierType.IsManual || (x.Enabled && x.ExecuteOnNewBeforeMetadata == (actionModifier != ActionModifierType.IsAfterMetadata)))
                .Aggregate(false, (current, conditionalAction) =>
                    current | conditionalAction.CheckAndExecute(game.Game, actionModifier == ActionModifierType.IsManual));

            if (Settings.WriteDebugLog)
            {
                Log.Debug($"==== Finished executing Conditional Actions on Game \"{game.Game.Name}\" ({mustUpdate}) ============================");

                foreach (IObjectType type in types)
                {
                    Log.Debug($"{type.LabelPlural} after: {string.Join(", ", type.LoadGameMetadata(game.Game).Select(x => x.Name))}");
                }
            }

            if (mustUpdate && actionModifier == ActionModifierType.IsManual)
            {
                _gamesAffected.Add(game.Game);
            }

            return mustUpdate;
        }

        public override void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            base.FollowUp(actionModifier, item, isBulkAction);

            _actions.Clear();
        }

        public override bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null,
            bool isBulkAction = true)
        {
            try
            {
                if (!base.Prepare(actionModifier, item, isBulkAction) || Settings.ConditionalActions.Count == 0)
                {
                    return false;
                }

                _actions.Clear();

                if (item is ConditionalAction singleAction)
                {
                    _actions.Add(singleAction);
                }
                else
                {
                    _actions.AddRange(Settings.ConditionalActions.OrderBy(ca => ca.SortNo).ThenBy(ca => ca.Name));
                }

                if (_actions.Count == 0)
                {
                    return false;
                }

                foreach (ConditionalAction conAction in _actions)
                {
                    foreach (Condition condition in conAction.Conditions)
                    {
                        condition.RefreshId();
                    }

                    foreach (Models.Action action in conAction.Actions)
                    {
                        action.AddToDb();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }

            return true;

        }
    }
}