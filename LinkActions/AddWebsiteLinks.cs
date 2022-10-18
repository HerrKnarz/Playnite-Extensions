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
        /// contains all website links that can be added.
        /// </summary>
        private readonly Links links;

        public string ProgressMessage { get; set; }
        public string ResultMessage { get; set; }
        public LinkUtilitiesSettings Settings { get; set; }

        public AddWebsiteLinks(LinkUtilitiesSettings settings)
        {
            ProgressMessage = "LOCLinkUtilitiesLibraryLinkProgress";
            ResultMessage = "LOCLinkUtilitiesAddedMessage";
            Settings = settings;

            links = new Links(Settings);
        }

        public bool Execute(Game game)
        {
            bool result = false;

            foreach (Linker.Link link in links)
            {
                result = result || link.AddLink(game);
            }

            return result;
        }
    }
}
