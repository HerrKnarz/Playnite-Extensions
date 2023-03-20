using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Base class for a library link
    /// </summary>
    internal abstract class LibraryLink : Link, IGameLibrary
    {
        public abstract Guid LibraryId { get; }

        public abstract bool AddLibraryLink(Game game);

        public LibraryLink(LinkUtilities plugin) : base(plugin)
        {
        }
    }
}
