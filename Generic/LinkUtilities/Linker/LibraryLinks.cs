using LinkUtilities.BaseClasses;
using LinkUtilities.Linker.Libraries;
using System;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// List of all game library link associations. Is used to get the specific library of the game via the GUID.
    /// </summary>
    internal class LibraryLinks : Dictionary<Guid, LibraryLink>
    {
        public LibraryLinks()
        {
            var steamLib = new LibraryLinkSteam();
            Add(steamLib.Id, steamLib);

            var gogLib = new LibraryLinkGog();
            Add(gogLib.Id, gogLib);

            var itchLib = new LibraryLinkItch();
            Add(itchLib.Id, itchLib);
        }
    }
}