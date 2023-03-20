using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to remove duplicate links.
    /// </summary>
    public class RemoveDuplicates : LinkAction
    {
        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressRemoveDuplicates";

        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogRemovedMessage";

        public RemoveDuplicates(LinkUtilities plugin) : base(plugin)
        {
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => LinkHelper.RemoveDuplicateLinks(game, Plugin.Settings.Settings.RemoveDuplicatesType);
    }
}
