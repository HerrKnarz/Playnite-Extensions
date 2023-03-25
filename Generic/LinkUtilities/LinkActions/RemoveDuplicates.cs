using LinkUtilities.Settings;

using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to remove duplicate links.
    /// </summary>
    internal class RemoveDuplicates : BaseClasses.LinkAction
    {
        private static RemoveDuplicates _instance = null;
        private static readonly object _mutex = new object();
        private RemoveDuplicates() : base()
        {
        }

        public static RemoveDuplicates Instance()
        {
            if (_instance == null)
            {
                lock (_mutex)
                {
                    if (_instance == null)
                    {
                        _instance = new RemoveDuplicates();
                    }
                }
            }

            return _instance;
        }

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressRemoveDuplicates";

        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogRemovedMessage";

        public bool RemoveDuplicatesAfterChange { get; set; } = false;

        public DuplicateTypes RemoveDuplicatesType { get; set; } = DuplicateTypes.NameAndUrl;

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => LinkHelper.RemoveDuplicateLinks(game, RemoveDuplicatesType);
    }
}
