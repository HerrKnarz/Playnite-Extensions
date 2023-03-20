using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Base class for a library link
    /// </summary>
    public abstract class LibraryLink : Link, IGameLibrary
    {
        public abstract Guid LibraryId { get; }

        public abstract bool AddLibraryLink(Game game);

        protected LibraryLink(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
