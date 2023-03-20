using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to rename links based on patterns.
    /// </summary>
    internal class RenameLinks : LinkAction
    {
        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressRenameLinks";

        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogRenamedMessage";

        /// <summary>
        /// List of patterns to find the links to rename based on URL or link name
        /// </summary>
        public LinkNamePatterns RenamePatterns { get; set; }

        public RenameLinks(LinkUtilities plugin) : base(plugin)
        {
        }

        private bool Rename(Game game, bool updateDB = true)
        {
            bool mustUpdate = false;

            if (game.Links != null && game.Links.Count > 0)
            {
                foreach (Link link in game.Links)
                {
                    string linkName = link.Name;

                    if (RenamePatterns.LinkMatch(ref linkName, link.Url))
                    {
                        if (linkName != link.Name)
                        {
                            API.Instance.MainView.UIDispatcher.Invoke(delegate
                            {
                                link.Name = linkName;
                            });

                            mustUpdate = true;
                        }
                    }
                }

                if (mustUpdate)
                {
                    // We start another renaming run, because there could be more links to rename after the last run
                    // renamed some links already.
                    Rename(game, false);

                    if (updateDB)
                    {
                        API.Instance.Database.Games.Update(game);
                    }
                }
            }

            return mustUpdate;
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => Rename(game);
    }
}
