using LinkUtilities.Linker;
using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to add a link to all available websites in the Links list, if a definitive link was found.
    /// </summary>
    public class AddWebsiteLinks : ILinkAction
    {
        /// <summary>
        /// contains all website Links that can be added.
        /// </summary>
        public readonly Links Links;

        public string ProgressMessage { get; } = "LOCLinkUtilitiesWebsiteLinkProgress";
        public string ResultMessage { get; } = "LOCLinkUtilitiesAddedMessage";
        public LinkUtilitiesSettings Settings { get; set; }

        public AddWebsiteLinks(LinkUtilitiesSettings settings)
        {
            Settings = settings;

            Links = new Links(Settings);
        }

        public bool Execute(Game game, string actionModifier = "")
        {
            bool result = false;

            foreach (Linker.Link link in Links)
            {
                if (actionModifier == "add")
                {
                    result = link.AddLink(game) || result;
                }
                else
                {
                    result = link.AddSearchedLink(game) || result;
                }
            }

            return result;
        }
    }
}
