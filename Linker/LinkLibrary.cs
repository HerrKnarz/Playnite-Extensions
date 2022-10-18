using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Base class for a library link
    /// </summary>
    public abstract class LibraryLink : ILink, IGameLibrary
    {
        public Guid LibraryId { get; set; }
        public string LinkName { get; set; }
        public string LinkUrl { get; set; }
        public LinkUtilitiesSettings Settings { get; set; }

        public abstract bool AddLink(Game game);
    }
}
