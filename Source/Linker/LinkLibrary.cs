using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Base class for a library link
    /// </summary>
    public abstract class LibraryLink : ILink, IGameLibrary
    {
        public abstract Guid LibraryId { get; }
        public abstract string LinkName { get; }
        public virtual string BaseUrl { get => string.Empty; }
        public virtual string LinkUrl { get; set; } = string.Empty;
        public LinkUtilitiesSettings Settings { get; set; }

        public abstract bool AddLink(Game game);

        public LibraryLink(LinkUtilitiesSettings settings)
        {
            Settings = settings;
        }
    }
}
