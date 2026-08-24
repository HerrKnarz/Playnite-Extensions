using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using LinkUtilities.Linker.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// List of all game library link associations. Is used to get the specific library of the game
    /// via the GUID.
    /// </summary>
    internal class LibraryLinks : Dictionary<Guid, LibraryLink>
    {
        public LibraryLinks()
        {
            var steamLib = new LibraryLinkSteam();
            Add(steamLib.LibraryIds.First(), steamLib);

            var gogLib = new LibraryLinkGog();
            Add(LinkHelper.GogId, gogLib);
            Add(LinkHelper.GogOssId, gogLib);

            var itchLib = new LibraryLinkItch();
            Add(itchLib.LibraryIds.First(), itchLib);
        }
    }
}
