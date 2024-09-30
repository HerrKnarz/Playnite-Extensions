using LinkUtilities.BaseClasses;
using LinkUtilities.Interfaces;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    ///     Class to rename links based on patterns.
    /// </summary>
    internal class RenameLinks : LinkAction
    {
        private static RenameLinks _instance;
        private RenameLinks() { }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressRenameLinks";

        public string RenameBlocker { get; set; } = string.Empty;

        public bool RenameLinksAfterChange { get; set; } = false;

        /// <summary>
        ///     List of patterns to find the links to rename based on URL or link name
        /// </summary>
        public LinkNamePatterns RenamePatterns { get; set; }

        public override string ResultMessage => "LOCLinkUtilitiesDialogRenamedMessage";

        public static RenameLinks Instance() => _instance ?? (_instance = new RenameLinks());

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => base.Execute(game, actionModifier, isBulkAction) &&
               Rename(game);

        private bool Rename(Game game, bool updateDb = true)
        {
            if (!game.Links?.Any() ?? true)
            {
                return false;
            }

            var mustUpdate = false;

            foreach (var link in game.Links)
            {
                if (RenameBlocker.Any() && link.Name.Contains(RenameBlocker))
                {
                    continue;
                }

                var linkName = link.Name;

                if (!RenamePatterns.LinkMatch(ref linkName, link.Url))
                {
                    continue;
                }

                if (linkName == link.Name)
                {
                    continue;
                }

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

            if (!mustUpdate)
            {
                return false;
            }

            // We start another renaming run, because there could be more links to rename after the last run
            // renamed some links already.
            Rename(game, false);

            if (updateDb && !GlobalSettings.Instance().OnlyATest)
            {
                API.Instance.Database.Games.Update(game);
            }

            return true;
        }
    }
}