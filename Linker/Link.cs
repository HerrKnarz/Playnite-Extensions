using LinkUtilities.LinkActions;
using Playnite.SDK.Models;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Base class for a website link, that's both used as an item in the Links list and 
    /// </summary>
    public abstract class Link : ILink, ILinkAction
    {
        public abstract string LinkName { get; }
        public virtual string BaseUrl { get; } = string.Empty;
        public virtual string LinkUrl { get; set; } = string.Empty;
        public string ProgressMessage { get; } = "LOCLinkUtilitiesLinkProgress";
        public string ResultMessage { get; } = "LOCLinkUtilitiesAddedMessage";
        public LinkUtilitiesSettings Settings { get; set; }

        public virtual bool AddLink(Game game)
        {
            return false;
        }

        public virtual bool Execute(Game game)
        {
            return AddLink(game);
        }

        public Link(LinkUtilitiesSettings settings)
        {
            Settings = settings;
        }
    }
}
