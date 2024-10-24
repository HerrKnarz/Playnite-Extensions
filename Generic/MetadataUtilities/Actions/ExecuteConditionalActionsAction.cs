using KNARZhelper;
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

        public static ExecuteConditionalActionsAction Instance() => _instance ?? (_instance = new ExecuteConditionalActionsAction());

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            var mustUpdate = _actions.Where(x => x.Enabled).Aggregate(false,
                (current, conditionalAction) => current |
                                                conditionalAction.CheckAndExecute(game.Game,
                                                    actionModifier == ActionModifierType.IsManual));

            if (mustUpdate && actionModifier != ActionModifierType.IsCombi)
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
                    _actions.AddRange(Settings.ConditionalActions);
                }

                if (_actions.Count == 0)
                {
                    return false;
                }

                foreach (var conAction in _actions)
                {
                    foreach (var condition in conAction.Conditions)
                    {
                        condition.RefreshId();
                    }

                    foreach (var action in conAction.Actions)
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