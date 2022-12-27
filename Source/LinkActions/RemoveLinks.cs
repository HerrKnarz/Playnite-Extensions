using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to remove unwanted links based on patterns.
    /// </summary>
    public class RemoveLinks : LinkAction
    {
        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressRemoveLinks";

        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogRemovedMessage";

        /// <summary>
        /// List of patterns to find the links to delete based on URL or link name
        /// </summary>
        public LinkNamePatterns RemovePatterns { get; set; }

        public RemoveLinks(LinkUtilities plugin) : base(plugin)
        {
        }

        public override bool Execute(Game game, string actionModifier = "", bool isBulkAction = true)
        {
            bool mustUpdate = false;

            if (game.Links != null && game.Links.Count > 0)
            {
                List<Link> links = game.Links.ToList();

                foreach (Link link in links)
                {
                    string linkName = link.Name;

                    if (RemovePatterns.LinkMatch(ref linkName, link.Url))
                    {
                        API.Instance.MainView.UIDispatcher.Invoke(delegate
                        {
                            mustUpdate = game.Links.Remove(link) || mustUpdate;
                        });
                    }
                }

                if (mustUpdate)
                {
                    API.Instance.Database.Games.Update(game);
                }
            }
            return mustUpdate;
        }
    }
}
