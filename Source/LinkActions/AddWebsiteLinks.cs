using LinkUtilities.Linker;
using Playnite.SDK.Models;

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
        public Links Links;

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
                if (actionModifier == "add" & link.Settings.IsAddable == true)
                {
                    result = link.AddLink(game) || result;
                }
                else if (actionModifier == "search" & link.Settings.IsSearchable == true)
                {
                    result = link.AddSearchedLink(game) || result;
                }
            }

            return result;
        }
    }
}
