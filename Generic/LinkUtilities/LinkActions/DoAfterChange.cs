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
        private DoAfterChange() : base()
        {
        }

        public static DoAfterChange Instance()
        {
            if (_instance == null)
            {
                lock (_mutex)
                {
                    if (_instance == null)
                    {
                        _instance = new DoAfterChange();
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

            if (RenameLinks.Instance().RenameLinksAfterChange && (RenameLinks.Instance().RenamePatterns?.Any() ?? false))
            {
                result = RenameLinks.Instance().Execute(game, actionModifier);
            }

            if (RemoveLinks.Instance().RemoveLinksAfterChange && (RemoveLinks.Instance().RemovePatterns?.Any() ?? false))
            {
                result |= RemoveLinks.Instance().Execute(game, actionModifier);
            }

            if (RemoveDuplicates.Instance().RemoveDuplicatesAfterChange)
            {
                result |= RemoveDuplicates.Instance().Execute(game, actionModifier);
            }

            if (SortLinks.Instance().SortAfterChange)
            {
                result |= SortLinks.Instance().Execute(game, actionModifier);
            }

            if (TagMissingLinks.Instance().TagMissingLinksAfterChange)
            {
                result |= TagMissingLinks.Instance().Execute(game, actionModifier);
            }

            return result;
        }
    }
}
