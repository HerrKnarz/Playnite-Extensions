using LinkUtilities.BaseClasses;
using LinkUtilities.Interfaces;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    ///     Class to remove unwanted links based on patterns.
    /// </summary>
    internal class RemoveLinks : LinkAction
    {
        private static RemoveLinks _instance;
        private RemoveLinks() { }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressRemoveLinks";

        public bool RemoveLinksAfterChange { get; set; } = false;

        /// <summary>
        ///     List of patterns to find the links to delete based on URL or link name
        /// </summary>
        public LinkNamePatterns RemovePatterns { get; set; }

        public override string ResultMessage => "LOCLinkUtilitiesDialogRemovedMessage";

        public static RemoveLinks Instance() => _instance ?? (_instance = new RemoveLinks());

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, isBulkAction))
            {
                return false;
            }

            var mustUpdate = false;

            if (!game.Links?.Any() ?? true)
            {
                return false;
            }

            var links = game.Links.ToList();

            foreach (var link in links)
            {
                var linkName = link.Name;

                if (!RemovePatterns.LinkMatch(ref linkName, link.Url))
                {
                    continue;
                }

                if (GlobalSettings.Instance().OnlyATest)
                {
                    mustUpdate |= game.Links.Remove(link);
                }
                else
                {
                    mustUpdate |= API.Instance.MainView.UIDispatcher.Invoke(() => game.Links.Remove(link));
                }
            }

            if (mustUpdate && !GlobalSettings.Instance().OnlyATest)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }
    }
}