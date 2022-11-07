using LinkUtilities.Linker;
using LinkUtilities.Models;
using Playnite.SDK.Models;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to add a link to all available websites in the Links list, if a definitive link was found.
    /// </summary>
    public class AddWebsiteLinks : LinkAction
    {
        /// <summary>
        /// contains all website Links that can be added.
        /// </summary>
        public readonly Links Links;

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressWebsiteLink";
        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogAddedMessage";

        public AddWebsiteLinks(LinkUtilities plugin) : base(plugin)
        {
            Links = new Links(Plugin);
        }

        public override bool Execute(Game game, string actionModifier = "")
        {
            bool result = false;

            foreach (Linker.Link link in Links)
            {
                LinkSourceSetting linkSetting = Plugin.Settings.Settings.LinkSourceSettings.FirstOrDefault(x => x.LinkName == link.LinkName);

                if (linkSetting != null)
                {
                    if (actionModifier == "add" & linkSetting.IsAddable == true & link.IsAddable)
                    {
                        result = link.AddLink(game) || result;
                    }
                    else if (actionModifier == "search" & linkSetting.IsSearchable == true & link.IsSearchable)
                    {
                        result = link.AddSearchedLink(game) || result;
                    }
                }
            }

            return result;
        }
    }
}
