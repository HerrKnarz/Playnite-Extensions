using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Windows;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to add a link from the clipboard.
    /// </summary>
    internal class AddLinkFromClipboard : LinkAction
    {
        private static AddLinkFromClipboard _instance;

        private AddLinkFromClipboard() { }

        /// <summary>
        /// Action that will be executed
        /// </summary>
        public ActionModifierTypes Action { get; set; }

        /// <summary>
        /// Name of the link to be added in the "AddLink" action
        /// </summary>
        public string LinkName { get; set; }

        /// <summary>
        /// List of patterns to find the right link name for a given URL
        /// </summary>
        public LinkNamePatterns LinkNamePatterns { get; set; }

        /// <summary>
        /// URL of the link to be added in the "AddLink" action
        /// </summary>
        public string LinkUrl { get; set; }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressWebsiteLink";

        public override string ResultMessage => "LOCLinkUtilitiesDialogAddedMessage";

        public static AddLinkFromClipboard Instance() => _instance ?? (_instance = new AddLinkFromClipboard());

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => base.Execute(game, actionModifier, isBulkAction) &&
               LinkHelper.AddLink(game, LinkName, LinkUrl, false);

        public override bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            LinkName = string.Empty;
            LinkUrl = string.Empty;

            var url = Clipboard.GetText();
            var tempLinkName = string.Empty;

            if (!url.Any() || !Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return false;
            }

            LinkUrl = url;

            if (LinkNamePatterns.LinkMatch(ref tempLinkName, url, true))
            {
                LinkName = tempLinkName;
                return true;
            }

            var selectResult = API.Instance.Dialogs.SelectString(
                ResourceProvider.GetString("LOCLinkUtilitiesDialogNameLinkText") + Environment.NewLine + url,
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