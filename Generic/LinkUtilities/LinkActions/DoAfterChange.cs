using Playnite.SDK.Models;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Sorts the Links of a game.
    /// </summary>
    internal class DoAfterChange : BaseClasses.LinkAction
    {
        private static DoAfterChange _instance = null;
        private static readonly object _mutex = new object();
        private DoAfterChange(LinkUtilities plugin) : base(plugin)
        {
        }

        public static DoAfterChange GetInstance(LinkUtilities plugin)
        {
            if (_instance == null)
            {
                lock (_mutex)
                {
                    if (_instance == null)
                    {
                        _instance = new DoAfterChange(plugin);
                    }
                }
            }

            return _instance;
        }

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressSortLinks";
        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogSortedMessage";

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            bool result = false;

            if (RenameLinks.GetInstance(Plugin).RenameLinksAfterChange && (RenameLinks.GetInstance(Plugin).RenamePatterns?.Any() ?? false))
            {
                result = RenameLinks.GetInstance(Plugin).Execute(game, actionModifier);
            }

            if (RemoveLinks.GetInstance(Plugin).RemoveLinksAfterChange && (RemoveLinks.GetInstance(Plugin).RemovePatterns?.Any() ?? false))
            {
                result |= RemoveLinks.GetInstance(Plugin).Execute(game, actionModifier);
            }

            if (RemoveDuplicates.GetInstance(Plugin).RemoveDuplicatesAfterChange)
            {
                result |= RemoveDuplicates.GetInstance(Plugin).Execute(game, actionModifier);
            }

            if (SortLinks.GetInstance(Plugin).SortAfterChange)
            {
                result |= SortLinks.GetInstance(Plugin).Execute(game, actionModifier);
            }

            if (TagMissingLinks.GetInstance(Plugin).TagMissingLinksAfterChange)
            {
                result |= TagMissingLinks.GetInstance(Plugin).Execute(game, actionModifier);
            }

            return result;
        }
    }
}
