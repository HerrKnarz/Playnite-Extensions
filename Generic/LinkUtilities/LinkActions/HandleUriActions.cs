using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System.Net;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to handle the actions received from the UriHandler. At the moment only adding links to the active URL in the
    /// web browser.
    /// </summary>
    internal class HandleUriActions : LinkAction
    {
        private static HandleUriActions _instance;
        private HandleUriActions() { }

        /// <summary>
        /// Action that will be executed
        /// </summary>
        public ActionModifierTypes Action { get; set; }

        /// <summary>
        /// Name of the link to be added in the "AddLink" action
        /// </summary>
        public string LinkName { get; set; }

        /// <summary>
        /// List of patterns to find the right link name for a given set of URL and link title
        /// </summary>
        public LinkNamePatterns LinkNamePatterns { get; set; }

        /// <summary>
        /// URL of the link to be added in the "AddLink" action
        /// </summary>
        public string LinkUrl { get; set; }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressWebsiteLink";

        public override string ResultMessage => "LOCLinkUtilitiesDialogAddedMessage";

        public static HandleUriActions Instance() => _instance ?? (_instance = new HandleUriActions());

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => actionModifier == ActionModifierTypes.Add &&
               base.Execute(game, actionModifier, isBulkAction) &&
               LinkHelper.AddLink(game, LinkName, LinkUrl, false);

        /// <summary>
        /// Processes the arguments received from the UriHandler.
        /// </summary>
        /// <param name="args">Arguments to process</param>
        /// <returns>True if the arguments could be processed and fit a LinkUtilities action</returns>
        public bool ProcessArgs(PlayniteUriEventArgs args)
        {
            switch (args.Arguments[0])
            {
                case "AddLink":
                    Action = ActionModifierTypes.Add;
                    break;
                default:
                    Action = ActionModifierTypes.None;
                    break;
            }

            if (Action != ActionModifierTypes.Add || args.Arguments.Length != 3)
            {
                return false;
            }

            var tempLinkName = args.Arguments[1];
            LinkUrl = WebUtility.UrlDecode(args.Arguments[2]);

            if (LinkNamePatterns.LinkMatch(ref tempLinkName, LinkUrl))
            {
                LinkName = tempLinkName;
                return true;
            }

            var selectResult = API.Instance.Dialogs.SelectString(
                ResourceProvider.GetString("LOCLinkUtilitiesDialogNameLinkText"),
                ResourceProvider.GetString("LOCLinkUtilitiesDialogNameLinkCaption"),
                tempLinkName);

            if (!selectResult.Result)
            {
                return false;
            }

            LinkName = selectResult.SelectedString;
            return true;
        }
    }
}