using LinkUtilities.Settings;
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
        /// URL of the link to be added in the "AddLink" action
        /// </summary>
        public string LinkUrl { get; set; }

        /// <summary>
        /// Name of the link to be added in the "AddLink" action
        /// </summary>
        public string LinkName { get; set; }

        /// <summary>
        /// Action that will be executed 
        /// </summary>
        public ActionModifierTypes Action { get; set; }

        /// <summary>
        /// List of patterns to find the right link name for a given set of URL and link title
        /// </summary>
        public LinkNamePatterns LinkNamePatterns { get; set; }

        /// <summary>
        /// Processes the arguments received from the UriHandler. 
        /// </summary>
        /// <param name="args">Arguments to process</param>
        /// <returns>True if the arguments could be processed and fit a LinkUtilities action</returns>
        public bool ProcessArgs(PlayniteUriEventArgs args)
        {
            bool result = false;

            switch (args.Arguments[0])
            {
                case "AddLink":
                    Action = ActionModifierTypes.Add;
                    break;
                default:
                    Action = ActionModifierTypes.None;
                    break;
            }

            if (Action == ActionModifierTypes.Add)
            {
                if (args.Arguments.Count() == 3)
                {
                    string tempLinkName = args.Arguments[1];
                    LinkUrl = WebUtility.UrlDecode(args.Arguments[2]);

                    if (LinkNamePatterns.LinkMatch(ref tempLinkName, LinkUrl))
                    {
                        LinkName = tempLinkName;
                        result = true;
                    }
                    else
                    {
                        StringSelectionDialogResult selectResult = API.Instance.Dialogs.SelectString(
                            ResourceProvider.GetString("LOCLinkUtilitiesDialogNameLinkText"),
                            ResourceProvider.GetString("LOCLinkUtilitiesDialogNameLinkCaption"),
                            tempLinkName);

                        if (selectResult.Result)
                        {
                            LinkName = selectResult.SelectedString;
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        public HandleUriActions(LinkUtilities plugin) : base(plugin)
        {
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            switch (actionModifier)
            {
                case ActionModifierTypes.Add:
                    return LinkHelper.AddLink(game, LinkName, LinkUrl, Plugin, false);
                default:
                    return false;
            }
        }
    }
}
