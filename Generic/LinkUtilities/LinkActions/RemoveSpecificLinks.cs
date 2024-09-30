using LinkUtilities.BaseClasses;
using LinkUtilities.Interfaces;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    internal class RemoveSpecificLinks : LinkAction
    {
        private static RemoveSpecificLinks _instance;
        private RemoveSpecificLinks() { }

        public List<SelectedLink> Links { get; set; } = new List<SelectedLink>();

        public override string ProgressMessage => "LOCLinkUtilitiesProgressRemoveLinks";

        public override string ResultMessage => "LOCLinkUtilitiesDialogRemovedMessage";

        public static RemoveSpecificLinks Instance() => _instance ?? (_instance = new RemoveSpecificLinks());

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

            var links = game.Links.Where(link => Links.Any(l => l.Selected && l.Name == link.Name)).ToList();

            foreach (var link in links)
            {
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
