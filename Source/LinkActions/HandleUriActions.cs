using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System.Linq;
using System.Net;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to handle the actions received from the UriHandler. At the moment only adding links to the active URL in the
    /// web browser.
    /// </summary>
    public class HandleUriActions : LinkAction
    {
        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressWebsiteLink";
        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogAddedMessage";
        /// <summary>
        /// Url of the link to be added in the "AddLink" action
        /// </summary>
        public string LinkUrl { get; set; }
        /// <summary>
        /// Name of the link to be added in the "AddLink" action
        /// </summary>
        public string LinkName { get; set; }
        /// <summary>
        /// Action that will be executed 
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Processes the arguments received from the UriHandler. 
        /// </summary>
        /// <param name="args">Arguments to process</param>
        /// <returns>True if the arguments could be processed and fit a LinkUtilities action</returns>
        public bool ProcessArgs(PlayniteUriEventArgs args)
        {
            bool result = false;

            Action = args.Arguments[0];

            if (Action == "AddLink")
            {
                if (args.Arguments.Count() == 3)
                {
                    LinkName = args.Arguments[1];
                    LinkUrl = WebUtility.UrlDecode(args.Arguments[2]);

                    StringSelectionDialogResult selectResult = API.Instance.Dialogs.SelectString(
                        ResourceProvider.GetString("LOCLinkUtilitiesDialogNameLinkText"),
                        ResourceProvider.GetString("LOCLinkUtilitiesDialogNameLinkCaption"),
                        LinkName);

                    if (selectResult.Result)
                    {
                        LinkName = selectResult.SelectedString;
                    }

                    result = true;
                }
            }

            return result;
        }

        public HandleUriActions(LinkUtilities plugin) : base(plugin)
        {
        }

        public override bool Execute(Game game, string actionModifier = "")
        {
            if (actionModifier == "AddLink")
            {
                return LinkHelper.AddLink(game, LinkName, LinkUrl, Plugin.Settings.Settings);
            }
            else
            {
                return false;
            }
        }
    }
}
