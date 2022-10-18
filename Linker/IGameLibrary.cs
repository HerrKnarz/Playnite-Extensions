using System;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Interface with properties needed for a game library
    /// </summary>
    interface IGameLibrary
    {
        /// <summary>
        /// ID of the game library (e.g. steam or gog) the link is part of. Is only used to add library links as of now.
        /// </summary>
        Guid LibraryId { get; }
    }
}
