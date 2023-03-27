using LinkUtilities.BaseClasses;
using System;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// List of all game library link associations. Is used to get the specific library of the game via the GUID.
    /// </summary>
    internal class Libraries : Dictionary<Guid, LibraryLink>
    {
        public Libraries()
        {
            Add(LibraryLinkSteam.Id, new LibraryLinkSteam());
            Add(LibraryLinkGog.Id, new LibraryLinkGog());
            Add(LibraryLinkItch.Id, new LibraryLinkItch());
        }
    }
}
