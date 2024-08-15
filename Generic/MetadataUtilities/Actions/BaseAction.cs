using Playnite.SDK.Models;

namespace MetadataUtilities.Actions
{
    public enum ActionModifierTypes
    {
        None,
        Add,
        Remove,
        Toggle,
        IsManual
    }

    public abstract class BaseAction : IBaseAction
    {
        public abstract string ProgressMessage { get; }

        public abstract string ResultMessage { get; }

        public virtual bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null,
            bool isBulkAction = true) => isBulkAction || Prepare(actionModifier, item, false);

        public virtual void FollowUp(ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null,
            bool isBulkAction = true)
        {
        }

        public virtual bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null,
            bool isBulkAction = true) => true;
    }
}