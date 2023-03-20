using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// List of all game library link associations. Is used to get the specific library of the game via the GUID using the find method.
    /// </summary>
    internal class Libraries : List<LibraryLink>
    {
        public Libraries(LinkUtilities plugin)
        {
            Add(new LibraryLinkSteam(plugin));
            Add(new LibraryLinkGog(plugin));
            Add(new LibraryLinkItch(plugin));
        }
    }
}
