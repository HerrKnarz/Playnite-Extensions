using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Sorts the Links of a game.
    /// </summary>
    internal class SortLinks : BaseClasses.LinkAction
    {
        private static SortLinks _instance = null;
        private static readonly object _mutex = new object();
        private SortLinks(LinkUtilities plugin) : base(plugin)
        {
        }

        public static SortLinks GetInstance(LinkUtilities plugin)
        {
            if (_instance == null)
            {
                lock (_mutex)
                {
                    if (_instance == null)
                    {
                        _instance = new SortLinks(plugin);
                    }
                }
            }

            return _instance;
        }

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressSortLinks";
        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogSortedMessage";

        public bool SortAfterChange { get; set; } = false;

        public bool UseCustomSortOrder { get; set; } = false;

        public Dictionary<string, int> SortOrder { get; set; }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            return actionModifier == ActionModifierTypes.SortOrder || (actionModifier == ActionModifierTypes.None && UseCustomSortOrder)
                ? LinkHelper.SortLinks(game, SortOrder)
                : LinkHelper.SortLinks(game);
        }
    }
}
