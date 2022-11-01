using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Acts as a combination of game library and website link source. Can add links to the game via library or direct link/search.
    /// </summary>
    public abstract class LinkAndLibrary : Link, ILibraryLink
    {
        protected LinkAndLibrary(LinkUtilities plugin) : base(plugin)
        {
        }

        public abstract Guid LibraryId { get; }

        public abstract bool AddLibraryLink(Playnite.SDK.Models.Game game);
    }
}
