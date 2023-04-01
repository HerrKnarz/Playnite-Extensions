using Playnite.SDK.Models;

namespace LinkUtilities.BaseClasses
{
    internal abstract class LinkAction : ILinkAction
    {
        public abstract string ProgressMessage { get; }

        public abstract string ResultMessage { get; }

        public abstract bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true);
    }
}