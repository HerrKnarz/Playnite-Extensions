using LinkUtilities.BaseClasses;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to rename links based on patterns.
    /// </summary>
    internal class RenameLinks : LinkAction
    {
        private static RenameLinks _instance;
        private static readonly object _mutex = new object();
        private RenameLinks() { }

        public static RenameLinks Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new RenameLinks();
                }
            }

            return _instance;
        }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressRenameLinks";

        public override string ResultMessage => "LOCLinkUtilitiesDialogRenamedMessage";

        public bool RenameLinksAfterChange { get; set; } = false;

        /// <summary>
        /// List of patterns to find the links to rename based on URL or link name
        /// </summary>
        public LinkNamePatterns RenamePatterns { get; set; }

        private bool Rename(Game game, bool updateDb = true)
        {
            bool mustUpdate = false;

            if (game.Links?.Any() ?? false)
            {
                foreach (Link link in game.Links)
                {
                    string linkName = link.Name;

                    if (RenamePatterns.LinkMatch(ref linkName, link.Url))
                    {
                        if (linkName != link.Name)
                        {
                            if (GlobalSettings.Instance().OnlyATest)
                            {
                                link.Name = linkName;
                            }
                            else
                            {
                                API.Instance.MainView.UIDispatcher.Invoke(delegate
                                {
                                    link.Name = linkName;
                                });
                            }

                            mustUpdate = true;
                        }
                    }
                }

                if (mustUpdate)
                {
                    // We start another renaming run, because there could be more links to rename after the last run
                    // renamed some links already.
                    Rename(game, false);

                    if (updateDb && !GlobalSettings.Instance().OnlyATest)
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