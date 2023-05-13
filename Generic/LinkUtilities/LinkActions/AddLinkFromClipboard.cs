using LinkUtilities.BaseClasses;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to add a link from the clipboard.
    /// </summary>
    internal class AddLinkFromClipboard : LinkAction
    {
        private static AddLinkFromClipboard _instance;
        private static readonly object _mutex = new object();

        private AddLinkFromClipboard() { }

        public static AddLinkFromClipboard Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new AddLinkFromClipboard();
                }
            }

            return _instance;
        }

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
        /// List of patterns to find the right link name for a given URL
        /// </summary>
        public LinkNamePatterns LinkNamePatterns { get; set; }

        public override bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            LinkName = string.Empty;
            LinkUrl = string.Empty;

            string url = Clipboard.GetText();
            string tempLinkName = string.Empty;

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

            StringSelectionDialogResult selectResult = API.Instance.Dialogs.SelectString(
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

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => base.Execute(game, actionModifier, isBulkAction) &&
               LinkHelper.AddLink(game, LinkName, LinkUrl, false);
    }
}