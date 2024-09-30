using LinkUtilities.Interfaces;
using Playnite.SDK.Models;

namespace LinkUtilities.BaseClasses
{
    internal abstract class LinkAction : ILinkAction
    {
        public abstract string ProgressMessage { get; }

        public abstract string ResultMessage { get; }

        public virtual bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => true;

        public virtual bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => isBulkAction || Prepare(actionModifier, false);
    }
}