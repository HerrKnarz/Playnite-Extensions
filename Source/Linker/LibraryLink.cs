using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Base class for a library link
    /// </summary>
    public abstract class LibraryLink : ILibraryLink
    {
        public abstract Guid LibraryId { get; }
        public abstract string LinkName { get; }
        public virtual string BaseUrl { get => string.Empty; }
        public virtual string LinkUrl { get; set; } = string.Empty;
        private readonly LinkUtilities plugin;
        public LinkUtilities Plugin { get { return plugin; } }

        public abstract bool AddLink(Game game);

        public abstract bool AddLibraryLink(Game game);

        public LibraryLink(LinkUtilities plugin)
        {
            this.plugin = plugin;
        }
    }
}
