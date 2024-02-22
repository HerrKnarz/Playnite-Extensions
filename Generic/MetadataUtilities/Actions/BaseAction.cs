using Playnite.SDK.Models;

namespace MetadataUtilities.Actions
{
    public enum ActionModifierTypes
    {
        None
    }

    public abstract class BaseAction : IBaseAction
    {
        public abstract string ProgressMessage { get; }

        public abstract string ResultMessage { get; }

        public virtual bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None,
            bool isBulkAction = true) => true;

        public virtual bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None,
            bool isBulkAction = true) => isBulkAction || Prepare(actionModifier, false);

        public virtual void FollowUp(ActionModifierTypes actionModifier = ActionModifierTypes.None,
            bool isBulkAction = true)
        {
        }
    }
}