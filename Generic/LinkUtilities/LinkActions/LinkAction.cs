using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    internal abstract class LinkAction : ILinkAction
    {
        public abstract string ProgressMessage { get; }

        public abstract string ResultMessage { get; }

        private readonly LinkUtilities _plugin;
        public LinkUtilities Plugin { get { return _plugin; } }

        public abstract bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true);

        public LinkAction(LinkUtilities plugin)
        {
            _plugin = plugin;
        }
    }
}
