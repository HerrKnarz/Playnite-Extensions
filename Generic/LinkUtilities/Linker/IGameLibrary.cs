using Playnite.SDK.Models;
using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Interface with properties needed for a game library
    /// </summary>
    internal interface IGameLibrary
    {
        /// <summary>
        /// ID of the game library (e.g. steam or gog) the link is part of. Is only used to add library Links as of now.
        /// </summary>
        Guid LibraryId { get; }

        /// <summary>
        /// Adds a link to the specific game page of the library.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <returns>
        /// True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        bool AddLibraryLink(Game game);
    }
}
