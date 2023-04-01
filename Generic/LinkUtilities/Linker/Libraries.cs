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
            LibraryLinkSteam steamLib = new LibraryLinkSteam();
            Add(steamLib.Id, steamLib);

            LibraryLinkGog gogLib = new LibraryLinkGog();
            Add(gogLib.Id, gogLib);

            LibraryLinkItch itchLib = new LibraryLinkItch();
            Add(itchLib.Id, itchLib);
        }
    }
}