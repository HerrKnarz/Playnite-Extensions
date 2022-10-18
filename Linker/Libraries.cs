﻿using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// List of all game library link associations. Is used to get the specific library of the game via the GUID using the find method.
    /// </summary>
    class Libraries : List<LibraryLink>
    {
        public Libraries(LinkUtilitiesSettings settings)
        {
            Add(new LibraryLinkSteam(settings));
            Add(new LibraryLinkGog(settings));
            Add(new LibraryLinkItch(settings));
        }
    }
}
