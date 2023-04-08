using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace LinkUtilities.BaseClasses
{
    /// <summary>
    /// Base class for a library link
    /// </summary>
    internal abstract class LibraryLink : Linker
    {
        public abstract Guid Id { get; }

        /// <summary>
        /// Adds a link to the specific game page of the library.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <param name="links">List of links to return (is usually only one for a library)</param>
        /// <returns>
        /// True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        public abstract bool FindLibraryLink(Game game, out List<Link> links);

        public override bool FindLinks(Game game, out List<Link> links)
            => game.PluginId == Id ? FindLibraryLink(game, out links) : base.FindLinks(game, out links);
    }
}