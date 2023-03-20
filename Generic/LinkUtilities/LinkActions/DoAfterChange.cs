using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Sorts the Links of a game.
    /// </summary>
    internal class DoAfterChange : LinkAction
    {
        internal DoAfterChange(LinkUtilities plugin) : base(plugin)
        {
        }

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressSortLinks";
        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogSortedMessage";

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            bool result = false;

            if (Plugin.Settings.Settings.RenameLinksAfterChange && Plugin.RenameLinks.RenamePatterns != null && Plugin.RenameLinks.RenamePatterns.Count > 0)
            {
                result = Plugin.RenameLinks.Execute(game, actionModifier);
            }

            if (Plugin.Settings.Settings.RemoveLinksAfterChange && Plugin.RemoveLinks.RemovePatterns != null && Plugin.RemoveLinks.RemovePatterns.Count > 0)
            {
                result = Plugin.RemoveLinks.Execute(game, actionModifier);
            }

            if (Plugin.Settings.Settings.RemoveDuplicatesAfterChange)
            {
                result = Plugin.RemoveDuplicates.Execute(game, actionModifier);
            }

            if (Plugin.Settings.Settings.SortAfterChange)
            {
                result = Plugin.SortLinks.Execute(game, actionModifier) || result;
            }

            if (Plugin.Settings.Settings.TagMissingLinksAfterChange)
            {
                result = Plugin.TagMissingLinks.Execute(game, actionModifier) || result;
            }

            return result;
        }
    }
}
