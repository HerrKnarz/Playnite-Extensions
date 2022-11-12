using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

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
        /// List of patterns to find the right link name for a given set of url and link title
        /// </summary>
        public static List<LinkNamePattern> LinkNamePatterns { get; set; }

        /// <summary>
        /// Fills the pattern list with default values
        /// </summary>
        public void FillDefaultLinkNamePatterns()
        {
            string json = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "DefaultLinkNamePatterns.json"));
            LinkNamePatterns = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LinkNamePattern>>(json);
        }

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
                    string tempLinkName = args.Arguments[1];
                    LinkUrl = WebUtility.UrlDecode(args.Arguments[2]);

                    LinkNamePattern pattern = LinkNamePatterns.FirstOrDefault(x => x.LinkMatch(tempLinkName, LinkUrl));
                    if (pattern != null)
                    {
                        LinkName = pattern.LinkName;
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
            //TODO: Move to Settings!
            FillDefaultLinkNamePatterns();
        }

        public override bool Execute(Game game, string actionModifier = "")
        {
            if (actionModifier == "AddLink")
            {
                return LinkHelper.AddLink(game, LinkName, LinkUrl, Plugin.Settings.Settings, false);
            }
            else
            {
                return false;
            }
        }
    }
}
