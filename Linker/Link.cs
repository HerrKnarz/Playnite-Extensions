using Playnite.SDK.Models;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Base class for a library link
    /// </summary>
    public abstract class Link : ILink
    {
        public abstract string LinkName { get; }
        public virtual string BaseUrl { get; } = string.Empty;
        public virtual string LinkUrl { get; set; } = string.Empty;
        public LinkUtilitiesSettings Settings { get; set; }

        public abstract bool AddLink(Game game);

        public Link(LinkUtilitiesSettings settings)
        {
            Settings = settings;
        }
    }
}
