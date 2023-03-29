using Playnite.SDK.Models;
using System;

namespace LinkUtilities.BaseClasses
{
    /// <summary>
    /// Base class for a library link
    /// </summary>
    internal abstract class LibraryLink : Link
    {
        public abstract Guid Id { get; }

        /// <summary>
        /// Adds a link to the specific game page of the library.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <returns>
        /// True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        public abstract bool AddLibraryLink(Game game);

        public override bool AddLink(Game game) => game.PluginId == Id ? AddLibraryLink(game) : base.AddLink(game);

        public LibraryLink() : base()
        {
        }
    }
}
