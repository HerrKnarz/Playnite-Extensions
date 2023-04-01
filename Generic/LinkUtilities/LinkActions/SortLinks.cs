using LinkUtilities.BaseClasses;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Sorts the Links of a game.
    /// </summary>
    internal class SortLinks : LinkAction
    {
        private static SortLinks _instance;
        private static readonly object _mutex = new object();
        private SortLinks() { }

        public static SortLinks Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new SortLinks();
                }
            }

            return _instance;
        }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressSortLinks";
        public override string ResultMessage => "LOCLinkUtilitiesDialogSortedMessage";

        public bool SortAfterChange { get; set; } = false;

        public bool UseCustomSortOrder { get; set; } = false;

        public Dictionary<string, int> SortOrder { get; set; }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => actionModifier == ActionModifierTypes.SortOrder || (actionModifier == ActionModifierTypes.None && UseCustomSortOrder)
                ? LinkHelper.SortLinks(game, SortOrder)
                : LinkHelper.SortLinks(game);
    }
}