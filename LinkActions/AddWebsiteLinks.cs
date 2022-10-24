﻿using LinkUtilities.Linker;
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

        public string ProgressMessage { get; } = "LOCLinkUtilitiesLibraryLinkProgress";
        public string ResultMessage { get; } = "LOCLinkUtilitiesAddedMessage";
        public LinkUtilitiesSettings Settings { get; set; }

        public AddWebsiteLinks(LinkUtilitiesSettings settings)
        {
            Settings = settings;

            links = new Links(Settings);
        }

        public bool Execute(Game game)
        {
            bool result = false;

            foreach (Linker.Link link in links)
            {
                result = link.AddLink(game) || result;
            }

            return result;
        }
    }
}
