using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    public abstract class LinkAction : ILinkAction
    {
        public abstract string ProgressMessage { get; }

        public abstract string ResultMessage { get; }

        private readonly LinkUtilities plugin;
        public LinkUtilities Plugin { get { return plugin; } }

        public abstract bool Execute(Game game, string actionModifier = "", bool isBulkAction = true);

        public LinkAction(LinkUtilities plugin)
        {
            this.plugin = plugin;
        }
    }
}
