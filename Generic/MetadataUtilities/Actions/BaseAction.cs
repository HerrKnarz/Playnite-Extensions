using MetadataUtilities.Enums;
using MetadataUtilities.Models;

namespace MetadataUtilities.Actions
{
    public abstract class BaseAction : IBaseAction
    {
        protected BaseAction(Settings settings) => Settings = settings;

        public abstract string ProgressMessage { get; }

        public abstract string ResultMessage { get; }

        public Settings Settings { get; set; }

        public virtual bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null,
            bool isBulkAction = true) => isBulkAction || Prepare(actionModifier, item, false);

        public virtual void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null,
            bool isBulkAction = true)
        {
        }

        public virtual bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null,
            bool isBulkAction = true) => true;
    }
}