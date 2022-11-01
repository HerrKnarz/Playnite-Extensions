using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Sorts the Links of a game.
    /// </summary>
    public class SortLinks : LinkAction
    {
        public SortLinks(LinkUtilities plugin) : base(plugin)
        {
        }

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressSortLinks";
        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogSortedMessage";

        public override bool Execute(Game game, string actionModifier = "")
        {
            return LinkHelper.SortLinks(game);
        }
    }
}
