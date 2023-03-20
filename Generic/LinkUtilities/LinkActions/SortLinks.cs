using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Sorts the Links of a game.
    /// </summary>
    internal class SortLinks : LinkAction
    {
        public SortLinks(LinkUtilities plugin) : base(plugin)
        {
        }

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressSortLinks";
        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogSortedMessage";

        public Dictionary<string, int> SortOrder { get; set; }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            if (actionModifier == ActionModifierTypes.SortOrder || (actionModifier == ActionModifierTypes.None && Plugin.Settings.Settings.UseCustomSortOrder))
            {
                return LinkHelper.SortLinks(game, SortOrder);
            }
            else
            {
                return LinkHelper.SortLinks(game);
            }
        }
    }
}
